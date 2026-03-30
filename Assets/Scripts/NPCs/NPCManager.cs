using System.Collections;
using System.Collections.Generic;
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

    private Queue<Customer> returnQueue = new Queue<Customer>();
    private bool isProcessingReturn = false;

    private void Start()
    {
        UpdateLinePositions();
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
        Customer firstCustomer = line[0];

        tipTimerSlider.value = firstCustomer.RemainingTipNormalized;

        // Show/hide slider based on whether the timer is active
        tipTimerSlider.gameObject.SetActive(firstCustomer.CanTip);
    }

    // Moves an NPC to the back of the line
    private IEnumerator ProcessReturns()
    {
        isProcessingReturn = true;

        while (returnQueue.Count > 0)
        {
            Customer cus = returnQueue.Dequeue();

            cus.MoveTo(backOfLine);

            // Wait while they are offscreen
            yield return new WaitForSeconds(offscreenWaitTime);

            // Add NPC back into the line
            line.Add(cus);

            int index = line.Count - 1;

            if (index < linePositions.Length)
            {
                cus.MoveTo(linePositions[index]);
            }
            else
            {
                cus.MoveTo(backOfLine);
                Debug.LogWarning("NPC returned but no line position available.");
            }
            yield return new WaitForSeconds(0.3f);
        }

        isProcessingReturn = false;
    }

    private void UpdateLinePositions()
    {
        int max = Mathf.Min(line.Count, linePositions.Length);

        for (int i = 0; i < max; i++)
        {
            line[i].MoveTo(linePositions[i]);

            if (i == 0)
                line[i].StartTipTimer();
            else
                line[i].StopTipTimer();
        }
    }

    private IEnumerator AdvanceLine(Customer cus)
    {
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
}
