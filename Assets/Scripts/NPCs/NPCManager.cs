using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;

public class NPCManager : MonoBehaviour
{
    [Header("Line Positioning")]
    [SerializeField] private List<Customer> line = new List<Customer>();
    [SerializeField] private Transform[] linePositions;
    [SerializeField] private Transform offScreenPosition;
    [SerializeField] private Transform hiddenPosition;
    [SerializeField] private Transform backOfLine;

    [Header("Visual Stuff")]
    [SerializeField] private float cupToHandTime = .5f;
    [SerializeField] private float offscreenWaitTime = 3f;

    [Header("UI")]
    [SerializeField] private Slider tipTimerSlider;

    private RegCustomerManager regularManager;

    private Queue<Customer> returnQueue = new Queue<Customer>();
    [SerializeField] private List<CustomerProfile> allProfiles;
    private bool isProcessingReturn = false;

    private void Awake()
    {
        regularManager = FindFirstObjectByType<RegCustomerManager>();
    }

    private void Start()
    {
        InitializeCustomers();
        UpdateLinePositions();
    }

    private void InitializeCustomers()
    {
        for (int i = 0; i < line.Count; i++)
        {
            CustomerProfile profile = GetRandomProfile();
            line[i].Initialize(profile);
        }
    }

    private CustomerProfile GetRandomProfile()
    {
        // Build a list of profiles not currently in line
        List<CustomerProfile> available = new List<CustomerProfile>();

        foreach (var profile in allProfiles)
        {
            bool alreadyInLine = false;

            foreach (var customer in line)
            {
                if (!customer.IsInitialized())
                    continue;

                if (customer.Profile == profile)
                {
                    alreadyInLine = true;
                    break;
                }
            }

            if (!alreadyInLine)
            {
                available.Add(profile);
            }
        }

        // if everything is already in line, allow duplicates. This shouldn't happen though.
        if (available.Count == 0)
        {
            available = allProfiles;
        }

        return available[Random.Range(0, available.Count)];
    }

    private void OnEnable()
    {
        SaleEvents.OnCupReady += OnCupReady;
        SaleEvents.OnCustomerTimedOut += OnCustomerTimedOut;
    }

    private void OnDisable()
    {
        SaleEvents.OnCupReady -= OnCupReady;
        SaleEvents.OnCustomerTimedOut -= OnCustomerTimedOut;
    }

    private void Update()
    {
        if (line.Count == 0)
            return;

        Customer firstCustomer = line[0];

        if (!firstCustomer.IsInitialized())
            return;

        tipTimerSlider.value = firstCustomer.RemainingTipNormalized;
        tipTimerSlider.gameObject.SetActive(firstCustomer.CanTip);

        // For testing
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ForceConvertFirstCustomer();
        }
    }

    // For testing
    private void ForceConvertFirstCustomer()
    {
        if (line.Count == 0)
            return;

        Customer firstCustomer = line[0];

        if (firstCustomer == null || !firstCustomer.IsInitialized())
            return;

        if (firstCustomer.IsRegular)
            return;

        regularManager.StartForceConversion(firstCustomer);
    }

    // Move NPC to back of line, replace with regular if possible
    private IEnumerator ProcessReturns()
    {
        isProcessingReturn = true;

        while (returnQueue.Count > 0)
        {
            Customer cus = returnQueue.Dequeue();

            cus.MoveTo(backOfLine);

            StartCoroutine(RefreshBeforeReturn(cus));

            line.Add(cus);

            int index = line.Count - 1;

            if (index < linePositions.Length)
                cus.MoveTo(linePositions[index]);
            else
                cus.MoveTo(backOfLine);

            yield return new WaitForSeconds(0.3f);
        }

        isProcessingReturn = false;
    }

    private void UpdateLinePositions()
    {
        int max = Mathf.Min(line.Count, linePositions.Length);

        for (int i = 0; i < max; i++)
        {
            Customer c = line[i];

            if (!c.IsInitialized())
                continue;

            c.MoveTo(linePositions[i]);

            if (i == 0)
                c.StartTipTimer();
            else
                c.StopTipTimer();
        }
    }

    private IEnumerator AdvanceLine(Customer cus)
    {
        cus.SetBusy(true);

        line.RemoveAt(0);

        // Move offscreen first
        cus.MoveTo(offScreenPosition);

        UpdateLinePositions();

        // Wait before teleporting to hidden staging position
        yield return new WaitForSeconds(offscreenWaitTime);

        cus.TeleportTo(hiddenPosition);

        returnQueue.Enqueue(cus);

        if (!isProcessingReturn)
            StartCoroutine(ProcessReturns());
    }

    private IEnumerator MoveCupToSlot(Customer cus, Cup cup)
    {
        if (cus == null || cup == null)
            yield break;

        cus.ReceiveCup(cup, cupToHandTime);
        SaleEvents.OnCupSold?.Invoke(cup, cus);

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(AdvanceLine(cus));

        yield return new WaitForSeconds(1f);

        cus.TeleportTo(hiddenPosition);
        Destroy(cup.gameObject);
    }

    private void OnCustomerTimedOut(Customer cus)
    {
        if (line.Count == 0 || line[0] != cus)
            return;

        // No money, just remove them like a failed sale
        StartCoroutine(AdvanceLine(cus));
    }

    public void OnCupReady(Cup cup, Customer customer)
    {
        if (!cup.IsReadyForSale)
            cup.MarkReadyForSale();

        if (line.Count == 0)
        {
            Debug.LogWarning("Cup ready but no customers in line.");
            return;
        }

        Customer firstCustomer = line[0];
        StartCoroutine(MoveCupToSlot(firstCustomer, cup));
    }

    private IEnumerator RefreshBeforeReturn(Customer cus)
    {
        yield return new WaitForSeconds(offscreenWaitTime);

        if (regularManager != null &&
            regularManager.TryGetRegular(out CustomerProfile regularProfile))
        {
            cus.SetRegular(true);
            cus.Initialize(regularProfile);
        }
        else
        {
            cus.SetRegular(false);
            cus.Initialize(GetRandomProfile());
        }
    }
}
