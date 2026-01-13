using System.Collections;
using System.Collections.Generic;
using UnityEditor.MPE;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [SerializeField] private List<NPCMover> line = new List<NPCMover>();
    [SerializeField] private Transform[] linePositions;
    [SerializeField] private Transform offScreenPosition;
    [SerializeField] private Transform hiddenPosition;
    [SerializeField] private Transform backOfLine;

    [SerializeField] private float cupToHandTime = .5f;
    [SerializeField] private float offscreenWaitTime = 3f;

    private Queue<NPCMover> returnQueue = new Queue<NPCMover>();
    private bool isProcessingReturn = false;


    public void OnCupSold(Cup cup)
    {
        StartCoroutine(moveNPCs(cup));
    }

    private IEnumerator moveNPCs(Cup cup)
    {
        // NPC pos 1 takes cup
        NPCMover npcToMove = line[0];
        StartCoroutine(MoveCupToSlot(npcToMove, cup));
        yield return new WaitForSeconds(.2f);

        // NPC moves off screen
        line.RemoveAt(0);
        npcToMove.MoveTo(offScreenPosition);

        // Move other NPCs
        UpdateLinePositions();

        // Wait a sec, teleport offscreen NPC to a hidden position at the end of the line. Remove cup while offscreen. 
        yield return new WaitForSeconds(1f);
        Destroy(cup.gameObject);
        npcToMove.TeleportTo(hiddenPosition);

        // Queue for line return
        returnQueue.Enqueue(npcToMove);

        // Start return processor if needed
        if (!isProcessingReturn)
        {
            StartCoroutine(ProcessReturns());
        }
    }

    private IEnumerator ProcessReturns()
    {
        isProcessingReturn = true;

        while (returnQueue.Count > 0)
        {
            NPCMover npc = returnQueue.Dequeue();
            npc.MoveTo(backOfLine);

            // Wait til we've reached the back of the line
            yield return new WaitForSeconds(offscreenWaitTime);

            // Get in line, move to first free pos in line
            line.Add(npc);
            npc.MoveTo(linePositions[line.Count - 1]);

            // Small delay before next NPC
            yield return new WaitForSeconds(0.3f);
        }

        isProcessingReturn = false;
    }

    private void UpdateLinePositions()
    {
        for(int i = 0; i< line.Count; i++)
        {
            line[i].MoveTo(linePositions[i]);
        }
    }

    private IEnumerator MoveCupToSlot(NPCMover npc, Cup cup)
    {
        Transform target = npc.GetCupSlot().transform;
        
        Vector3 startPos = cup.transform.position;
        Quaternion startRot = cup.transform.rotation;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / cupToHandTime;

            cup.transform.position = Vector3.Lerp(startPos, target.position, t);
            cup.transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);

            yield return null;
        }

        // Snap cleanly at the end
        cup.transform.position = target.position;
        cup.transform.rotation = target.rotation;

        // Optional: parent it so it stays with the NPC
        cup.transform.SetParent(target);
    }

    private void OnEnable()
    {
        SaleEvents.OnCupSold += OnCupSold;
    }

    private void OnDisable()
    {
        SaleEvents.OnCupSold -= OnCupSold;
    }

}
