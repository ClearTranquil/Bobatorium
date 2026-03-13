using UnityEngine;
using System.Collections;

public class Customer : MonoBehaviour, ICustomerInfo
{
    [Header("Data")]
    [Range(0f, 1f)]
    [SerializeField] private float baseTipChance = 0.1f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arrivalThreshold = 0.05f;
    private Transform target;

    [SerializeField] private Transform cupSlot;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        // Offset idle anim so customers don't move in sync
        float randomOffset = Random.value;
        animator.Play("idle", 0, randomOffset);
    }

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

        animator.SetBool("walking", true);
    }

    public void TeleportTo(Transform newTarget)
    {
        target = null;
        transform.position = newTarget.position;

        animator.SetBool("walking", true);
    }

    private void Update()
    {
        if (!target)
            return;

        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) <= arrivalThreshold)
        {
            target = null;
            animator.SetBool("walking", false);
        }
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
