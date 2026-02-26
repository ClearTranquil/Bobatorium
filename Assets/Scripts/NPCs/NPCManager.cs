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

        // Step 1: Customer takes the cup
        cus.ReceiveCup(cup, cupToHandTime);

        // Step 2: Small buffer so customer can start receiving
        yield return new WaitForSeconds(0.2f);

        // Step 3: Remove customer from front of line and move offscreen
        line.RemoveAt(0);
        cus.MoveTo(offScreenPosition);

        // Step 4: Move other customers forward
        UpdateLinePositions();

        // Step 5: Wait a bit before teleporting offscreen customer
        yield return new WaitForSeconds(1f);

        // Step 6: Teleport customer to hidden position
        cus.TeleportTo(hiddenPosition);

        // Step 7: Remove cup now (cup is destroyed while customer is offscreen)
        Destroy(cup.gameObject);

        // Step 8: Queue for return
        returnQueue.Enqueue(cus);

        // Step 9: Start return processor if needed
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
