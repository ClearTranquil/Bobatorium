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
        if (line.Count == 0)
        {
            Debug.LogWarning("Cup sold but no NPCs in line.");
            yield break;
        }

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

            // Wait while they are offscreen
            yield return new WaitForSeconds(offscreenWaitTime);

            // Add NPC back into the line
            line.Add(npc);

            int index = line.Count - 1;

            if (index < linePositions.Length)
            {
                npc.MoveTo(linePositions[index]);
            }
            else
            {
                npc.MoveTo(backOfLine);
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
