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



    private IEnumerator MoveCupToSlot(Customer cus, Cup cup)
    {
        if (cus == null || cup == null)
            yield break;

        // Customer takes the cup, invoke cup sale
        cus.ReceiveCup(cup, cupToHandTime);
        SaleEvents.OnCupSold?.Invoke(cup, cus);

        // Small buffer so customer can start receiving
        yield return new WaitForSeconds(0.2f);

        // Remove customer from front of line and move offscreen
        line.RemoveAt(0);
        cus.MoveTo(offScreenPosition);

        // Move other customers forward
        UpdateLinePositions();
        yield return new WaitForSeconds(1f);

        // Teleport customer to hidden position at end of line, remove their cup
        cus.TeleportTo(hiddenPosition);
        Destroy(cup.gameObject);

        // Get back in line, chump
        returnQueue.Enqueue(cus);

        if (!isProcessingReturn)
            StartCoroutine(ProcessReturns());
    }

    private void OnEnable()
    {
        SaleEvents.OnCupReady += OnCupReady;
    }

    private void OnDisable()
    {
        SaleEvents.OnCupReady -= OnCupReady;
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
