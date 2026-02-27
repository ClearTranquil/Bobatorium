using System.Collections;
using System.Collections.Generic;
using UnityEditor.MPE;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [SerializeField] private List<Customer> line = new List<Customer>();
    [SerializeField] private Transform[] linePositions;
    [SerializeField] private Transform offScreenPosition;
    [SerializeField] private Transform hiddenPosition;
    [SerializeField] private Transform backOfLine;

    [SerializeField] private float cupToHandTime = .5f;
    [SerializeField] private float offscreenWaitTime = 3f;

    private Queue<Customer> returnQueue = new Queue<Customer>();
    private bool isProcessingReturn = false;

    //private IEnumerator moveNPCs(Cup cup)
    //{
    //    if (line.Count == 0)
    //    {
    //        Debug.LogWarning("Cup sold but no NPCs in line.");
    //        yield break;
    //    }

    //    // NPC pos 1 takes cup
    //    Customer cusToMove = line[0];
    //    StartCoroutine(MoveCupToSlot(cusToMove, cup));
    //    yield return new WaitForSeconds(.2f);

    //    // NPC moves off screen
    //    line.RemoveAt(0);
    //    cusToMove.MoveTo(offScreenPosition);

    //    // Move other NPCs
    //    UpdateLinePositions();

    //    // Wait a sec, teleport offscreen NPC to a hidden position at the end of the line. Remove cup while offscreen. 
    //    yield return new WaitForSeconds(1f);
    //    Destroy(cup.gameObject);
    //    cusToMove.TeleportTo(hiddenPosition);

    //    // Queue for line return
    //    returnQueue.Enqueue(cusToMove);

    //    // Start return processor if needed
    //    if (!isProcessingReturn)
    //    {
    //        StartCoroutine(ProcessReturns());
    //    }
    //}

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
        }
    }



    private IEnumerator MoveCupToSlot(Customer cus, Cup cup)
    {
        if (cus == null || cup == null || line.Count == 0)
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

        Customer firstCustomer = line[0];
        StartCoroutine(MoveCupToSlot(firstCustomer, cup));
    }
}
