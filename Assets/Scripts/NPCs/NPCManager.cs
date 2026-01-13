using System.Collections;
using System.Collections.Generic;
using UnityEditor.MPE;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public List<NPCMover> line = new List<NPCMover>();
    public Transform[] linePositions;
    public Transform offScreenPosition;
    public Transform hiddenPosition;
    public Transform backOfLine;

    private Queue<NPCMover> returnQueue = new Queue<NPCMover>();
    private bool isProcessingReturn = false;

    public float offscreenWaitTime = 3f;

    public void OnCupSold(Cup cup)
    {
        StartCoroutine(moveNPCs(cup));
    }

    private IEnumerator moveNPCs(Cup cup)
    {
        // NPC in pos 1 takes cup, moves off screen
        NPCMover npcToMove = line[0];
        line.RemoveAt(0);
        npcToMove.MoveTo(offScreenPosition);

        // Move other NPCs
        UpdateLinePositions();

        // Wait a sec, teleport offscreen NPC to a hidden position at the end of the line
        yield return new WaitForSeconds(1f);
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

    private void OnEnable()
    {
        SaleEvents.OnCupSold += OnCupSold;
    }

    private void OnDisable()
    {
        SaleEvents.OnCupSold -= OnCupSold;
    }

}
