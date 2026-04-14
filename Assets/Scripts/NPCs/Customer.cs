using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Customer : MonoBehaviour, ICustomerInfo
{

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arrivalThreshold = 0.05f;
    private Transform target;
    public bool IsBusy { get; private set; }

    [Header("Animation/Visuals")]
    [SerializeField] private Transform cupSlot;
    private Animator animator;
    private float idleOffset;
    [SerializeField] private SpriteRenderer headRenderer;
    [SerializeField] private SpriteRenderer bodyRenderer;
    private bool initialized = false;

    [Header("Tip timer")]
    private float remainingTipTime;
    private bool timerRunning;

    [Header("Regulars")]
    [SerializeField] private bool isRegular = false;
    [SerializeField] private GameObject regularStar;

    public float TipTime => Profile != null ? Profile.tipTime : 0f;
    public bool CanTip => remainingTipTime > 0f;
    public float RemainingTipNormalized => remainingTipTime / TipTime;
    public bool WasServedInTime { get; private set; }
    public bool IsRegular => isRegular;
    public CustomerProfile Profile { get; private set; }


    private void Awake()
    {
        animator = GetComponent<Animator>();

        // Offset idle anim so customers don't move in sync
        idleOffset = Random.value;
        animator.Play("idle", 0, idleOffset);

        IsBusy = false;

    }

    /*==========Customer Data============*/
    public float BaseTipChance => Profile != null ? Profile.baseTipChance : 0f;
    public Transform CupSlot => cupSlot;

    public float GetTipChance()
    {
        return BaseTipChance;
    }

    public void Initialize(CustomerProfile profile)
    {
        Profile = profile;

        ApplyProfile(profile);
        initialized = true;
    }

    public bool IsInitialized()
    {
        return Profile != null;
    }

    /*===========Movement===========*/

    public void SetBusy(bool value)
    {
        IsBusy = value;
    }

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
        if (!initialized)
            return;

        if (timerRunning)
        {
            remainingTipTime -= Time.deltaTime;

            // Timer ran out, penalize the customer satisfaction
            if (remainingTipTime <= 0f)
            {
                remainingTipTime = 0f;
                timerRunning = false;

                SaleEvents.OnCustomerTimedOut?.Invoke(this);
            }
        }

        if (!target)
            return;

        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // Checks if customer has arrived at their destination
        if (Vector3.Distance(transform.position, target.position) <= arrivalThreshold)
        {
            target = null;
            animator.SetBool("walking", false);
            idleOffset = Random.value;
            animator.Play("idle", 0, idleOffset);
        }
    }

    /*============Customer Logic============*/

    public void ReceiveCup(Cup cup, float moveTime = 0.5f)
    {
        WasServedInTime = remainingTipTime > 0f;

        StopTipTimer();
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

    /*============Tipping============*/
    public void StartTipTimer()
    {
        remainingTipTime = TipTime;
        timerRunning = true;
    }

    public void StopTipTimer()
    {
        timerRunning = false;
    }

    /*=================Regulars===================*/
    public void SetRegular(bool value)
    {
        isRegular = value;

        if (isRegular)
        {
            regularStar.SetActive(true);
        }
    }

    /*=============Visuals=====================*/
    public void ApplyProfile(CustomerProfile profile)
    {
        headRenderer.sprite = profile.head;
        bodyRenderer.sprite = profile.body;
    }
}
