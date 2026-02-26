using UnityEngine;
using System.Collections;

public class Customer : MonoBehaviour, ICustomerInfo
{
    [Header("Data")]
    [Range(0f, 1f)]
    [SerializeField] private float baseTipChance = 0.1f;

    [SerializeField] private Transform cupSlot;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    private Transform target;

    /*==========Customer Data============*/
    public float BaseTipChance => baseTipChance;
    public Transform CupSlot => cupSlot;

    public float GetTipChance()
    {
        return BaseTipChance;
    }

    /*===========Movement===========*/
    public void MoveTo(Transform newTarget)
    {
        target = newTarget;
    }

    public void TeleportTo(Transform newTarget)
    {
        target = null;
        transform.position = newTarget.position;
    }

    private void Update()
    {
        if (!target)
            return;

        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
    }

    /*============Customer Logic============*/
     
    public void ReceiveCup(Cup cup, float moveTime = 0.5f)
    {
        StartCoroutine(MoveCupToHand(cup, moveTime));
    }

    private IEnumerator MoveCupToHand(Cup cup, float duration)
    {
        if (!cup) yield break;

        cup.TogglePhysics(false);

        Vector3 startPos = cup.transform.position;
        Quaternion startRot = cup.transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cup.transform.position = Vector3.Lerp(startPos, cupSlot.position, t);
            cup.transform.rotation = Quaternion.Slerp(startRot, cupSlot.rotation, t);
            yield return null;
        }

        cup.transform.position = cupSlot.position;
        cup.transform.rotation = cupSlot.rotation;
        cup.transform.SetParent(cupSlot);

        // Mark cup ready for sale after it reaches the hand
        cup.MarkReadyForSale();
    }
}
