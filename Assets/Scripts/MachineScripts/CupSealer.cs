using UnityEngine;
using System.Collections;

public class CupSealer : Machine
{
    [Header("Base states")]
    private float baseClawSpeedMult = 1f;
    private float baseRotationSpeedMult = 1f;
    private float clawSpeedMult = 1f;
    private float rotationSpeedMult = 1f;
    [SerializeField] private float baseClawDuration = .5f;
    [SerializeField] private float baseRotationDuration = 1.5f;

    [Header("Animation Variables")]
    [SerializeField] private float liftHeight = 0.5f;
    [SerializeField] private float armLowerDistance = 0.2f;
    [SerializeField] private float armGripDistance = 0.1f;

    [Header("Claws")]
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;
    private Vector3 leftArmRestPos;
    private Vector3 rightArmRestPos;

    private bool isProcessing = false;

    protected override void Awake()
    {
        base.Awake();

        clawSpeedMult = baseClawSpeedMult;
        rotationSpeedMult = baseRotationSpeedMult;

        leftArmRestPos = leftArm.localPosition;
        rightArmRestPos = rightArm.localPosition;
    }

    protected override bool HandleUpgradeEvent(Machine m_machine, Upgrade m_upgrade, int m_newLevel)
    {
        base.HandleUpgradeEvent(m_machine, m_upgrade, m_newLevel);

        if (m_upgrade.upgradeID == "ClawArmSpeed")
            clawSpeedMult = m_upgrade.stackValues[m_newLevel - 1];

        if (m_upgrade.upgradeID == "RotSpeed")
            rotationSpeedMult = m_upgrade.stackValues[m_newLevel - 1];

        return true;
    }

    public override void TriggerAction()
    {
        base.TriggerAction();

        // Ignore input if machine is already processing cups
        if (isProcessing) return;

        bool foundCup = false;
        foreach (CupSnapPoint snap in cupSnapPoints)
        {
            if (snap.Occupant != null && !snap.IsBusy)
            {
                StartCoroutine(SealRoutine(snap));
                foundCup = true;

                // Prevent grabbing cup until process is done
                snap.Occupant.SetGrabEnabled(false);
            }
        }

        if (foundCup)
            isProcessing = true;
    }

    private IEnumerator SealRoutine(CupSnapPoint snap)
    {
        snap.IsBusy = true;

        Cup cup = snap.Occupant;
        Transform snapTransform = snap.transform;

        if(leftArm && rightArm)
            yield return ClampCup(snapTransform, cup);

        yield return LiftCup(snapTransform);

        yield return RotateCup(snapTransform);

        cup.SealCup();

        yield return LowerCup(snapTransform);

        if (leftArm && rightArm)
            yield return ReleaseCup(snapTransform, cup);

        snap.IsBusy = false;

        // Wait til all cup slots are finished processing before accepting input again
        bool machineBusy = false;
        foreach (var s in cupSnapPoints)
        {
            if (s.IsBusy)
            {
                machineBusy = true;
                break;
            }
        }

        if (!machineBusy)
            isProcessing = false;
    }

    /*----------------------Coroutine Steps----------------------*/

    private IEnumerator ClampCup(Transform snapTransform, Cup cup)
    {
        // Lower the claw arms to meet the cup
        
        float downDuration = (baseClawDuration / 2) / clawSpeedMult;
        Vector3 leftDownTarget = leftArmRestPos + Vector3.down * armLowerDistance;
        Vector3 rightDownTarget = rightArmRestPos + Vector3.down * armLowerDistance;

        Vector3 leftStart = leftArm.localPosition;
        Vector3 rightStart = rightArm.localPosition;

        float elapsed = 0f;
        while (elapsed < downDuration)
        {
            float t = elapsed / downDuration;
            leftArm.localPosition = Vector3.Lerp(leftStart, leftDownTarget, t);
            rightArm.localPosition = Vector3.Lerp(rightStart, rightDownTarget, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        leftArm.localPosition = leftDownTarget;
        rightArm.localPosition = rightDownTarget;

        // Move the claw arms inwards to grip the cup

        float inwardDuration = baseClawDuration / clawSpeedMult;
        Vector3 leftGripTarget = leftDownTarget + Vector3.right * armGripDistance;
        Vector3 rightGripTarget = rightDownTarget + Vector3.left * armGripDistance;

        leftStart = leftArm.localPosition;
        rightStart = rightArm.localPosition;
        elapsed = 0f;

        while (elapsed < inwardDuration)
        {
            float t = elapsed / inwardDuration;
            leftArm.localPosition = Vector3.Lerp(leftStart, leftGripTarget, t);
            rightArm.localPosition = Vector3.Lerp(rightStart, rightGripTarget, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        leftArm.localPosition = leftGripTarget;
        rightArm.localPosition = rightGripTarget;
    }

    private IEnumerator LiftCup(Transform snapTransform)
    {
        // Once cup is gripped, move cup upwards

        float duration = baseClawDuration / clawSpeedMult;
        float elapsed = 0f;

        Vector3 cupStart = snapTransform.position;
        Vector3 cupTarget = cupStart + Vector3.up * liftHeight;

        Vector3 leftStart = leftArm.localPosition;
        Vector3 rightStart = rightArm.localPosition;

        Vector3 leftTarget = leftStart + Vector3.up * liftHeight;
        Vector3 rightTarget = rightStart + Vector3.up * liftHeight;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            snapTransform.position = Vector3.Lerp(cupStart, cupTarget, t);
            if (leftArm && rightArm)
            {
                leftArm.localPosition = Vector3.Lerp(leftStart, leftTarget, t);
                rightArm.localPosition = Vector3.Lerp(rightStart, rightTarget, t);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        snapTransform.position = cupTarget;
        if (leftArm && rightArm)
        {
            leftArm.localPosition = leftTarget;
            rightArm.localPosition = rightTarget;
        }
    }

    private IEnumerator RotateCup(Transform snapTransform)
    {
        // Play a spinning animation, like lid is being sealed on

        float duration = baseRotationDuration / rotationSpeedMult;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            snapTransform.Rotate(0f, 360f * Time.deltaTime / duration, 0f, Space.Self);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator LowerCup(Transform snapTransform)
    {
        // Once cup is sealed, put it back down

        float duration = baseClawDuration / clawSpeedMult;
        float elapsed = 0f;

        Vector3 cupStart = snapTransform.position;
        Vector3 cupTarget = cupStart - Vector3.up * liftHeight;

        Vector3 leftStart = leftArm.localPosition;
        Vector3 rightStart = rightArm.localPosition;

        Vector3 leftTarget = leftStart - Vector3.up * liftHeight;
        Vector3 rightTarget = rightStart - Vector3.up * liftHeight;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            snapTransform.position = Vector3.Lerp(cupStart, cupTarget, t);
            if (leftArm && rightArm)
            {
                leftArm.localPosition = Vector3.Lerp(leftStart, leftTarget, t);
                rightArm.localPosition = Vector3.Lerp(rightStart, rightTarget, t);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        snapTransform.position = cupTarget;
        if (leftArm && rightArm)
        {
            leftArm.localPosition = leftTarget;
            rightArm.localPosition = rightTarget;
        }
    }

    private IEnumerator ReleaseCup(Transform snapTransform, Cup cup)
    {
        // Move arms apart to "unclamp" the cup

        float outwardDuration = (baseClawDuration / 2) / clawSpeedMult;

        Vector3 leftOutTarget = leftArm.localPosition + Vector3.left * armGripDistance;
        Vector3 rightOutTarget = rightArm.localPosition + Vector3.right * armGripDistance;

        Vector3 leftStart = leftArm.localPosition;
        Vector3 rightStart = rightArm.localPosition;

        float elapsed = 0f;
        while (elapsed < outwardDuration)
        {
            float t = elapsed / outwardDuration;
            leftArm.localPosition = Vector3.Lerp(leftStart, leftOutTarget, t);
            rightArm.localPosition = Vector3.Lerp(rightStart, rightOutTarget, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        leftArm.localPosition = leftOutTarget;
        rightArm.localPosition = rightOutTarget;

        // Allow cup to be grabbed at this point
        cup.SetGrabEnabled(true);

        // Move arms back to their rest position

        float upDuration = baseClawDuration / clawSpeedMult;

        leftStart = leftArm.localPosition;
        rightStart = rightArm.localPosition;
        elapsed = 0f;

        while (elapsed < upDuration)
        {
            float t = elapsed / upDuration;
            leftArm.localPosition = Vector3.Lerp(leftStart, leftArmRestPos, t);
            rightArm.localPosition = Vector3.Lerp(rightStart, rightArmRestPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        leftArm.localPosition = leftArmRestPos;
        rightArm.localPosition = rightArmRestPos;

    }
}
