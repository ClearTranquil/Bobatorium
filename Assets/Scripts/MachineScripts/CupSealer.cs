using UnityEngine;
using System.Collections;

public class CupSealer : Machine
{
    [SerializeField] private GameObject slotUpgrade1;
    [SerializeField] private GameObject slotUpgrade2;
    
    [Header("Base states")]
    //private float baseClawSpeedMult = 1f;
    //private float baseRotationSpeedMult = 1f;
    private float clawSpeedMult = 1f;
    private float rotationSpeedMult = 1f;
    [SerializeField] private float baseClawDuration = .5f;
    [SerializeField] private float baseRotationDuration = 1.5f;

    [Header("Animation Variables")]
    [SerializeField] private float liftHeight = 0.5f;
    [SerializeField] private float armLowerDistance = 0.2f;
    [SerializeField] private float armGripDistance = 0.1f;

    [Header("Claws")]
    [SerializeField] private Transform[] leftArms;
    [SerializeField] private Transform[] rightArms;
    private Vector3[] leftRestPositions;
    private Vector3[] rightRestPositions;

    [Header("Employee interaction")]
    [SerializeField] private float timeBetweenEmployeeTrigger;
    private bool isBeingWorked = false;
    private bool isProcessing = false;

    protected override void Awake()
    {
        base.Awake();

        leftRestPositions = new Vector3[leftArms.Length];
        rightRestPositions = new Vector3[rightArms.Length];

        for (int i = 0; i < leftArms.Length; i++)
            leftRestPositions[i] = leftArms[i].localPosition;

        for (int i = 0; i < rightArms.Length; i++)
            rightRestPositions[i] = rightArms[i].localPosition;
    }

    protected override bool HandleUpgradeEvent(Machine m_machine, Upgrade m_upgrade, int m_newLevel)
    {
        base.HandleUpgradeEvent(m_machine, m_upgrade, m_newLevel);

        if (m_upgrade.upgradeID == "ClawArmSpeed")
            clawSpeedMult = m_upgrade.stackValues[m_newLevel - 1];

        if (m_upgrade.upgradeID == "RotSpeed")
            rotationSpeedMult = m_upgrade.stackValues[m_newLevel - 1];
        
        if (m_upgrade.upgradeID == "AddCupSlot")
        {
            ActivateSealer(Mathf.RoundToInt(m_upgrade.stackValues[m_newLevel - 1]));
            return true;
        }
        return true;
    }

    private void ActivateSealer(int upgradeLevel)
    {
        switch (upgradeLevel)
        {
            case 1:
                slotUpgrade1.SetActive(true);
                cupSnapPoints[1].gameObject.SetActive(true);
                Debug.Log("Activating cup slot 1");
                return;
            case 2:
                slotUpgrade2.SetActive(true);
                cupSnapPoints[2].gameObject.SetActive(true);
                Debug.Log("Activating cup slot 2");
                return;
        }
    }

    public override bool CheckCupCompletion()
    {
        foreach (var snap in cupSnapPoints)
        {
            ICupInfo cup = snap;
            if (cup != null)
            {
                if (cup.IsSealed)
                    return true;
            }
        }

        return false;
    }

    public override bool CheckSpecificCupCompletion(Cup cup)
    {
        return cup.IsSealed;
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

        if (leftArms != null && rightArms != null && leftArms.Length > 0 && rightArms.Length > 0)
            yield return ClampCup(snapTransform, cup);

        yield return LiftCup(snapTransform);

        yield return RotateCup(snapTransform);

        cup.SealCup();

        yield return LowerCup(snapTransform);

        if (leftArms != null && rightArms != null && leftArms.Length > 0 && rightArms.Length > 0)
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
    // TO DO: Clean all this mess up
    private IEnumerator ClampCup(Transform snapTransform, Cup cup)
    {
        // Lower the claw arms
        float downDuration = (baseClawDuration / 2f) / clawSpeedMult;
        float elapsed = 0f;

        Vector3[] leftStart = new Vector3[leftArms.Length];
        Vector3[] rightStart = new Vector3[rightArms.Length];
        Vector3[] leftDownTarget = new Vector3[leftArms.Length];
        Vector3[] rightDownTarget = new Vector3[rightArms.Length];

        for (int i = 0; i < leftArms.Length; i++)
        {
            leftStart[i] = leftArms[i].localPosition;
            rightStart[i] = rightArms[i].localPosition;

            leftDownTarget[i] = leftStart[i] + Vector3.down * armLowerDistance;
            rightDownTarget[i] = rightStart[i] + Vector3.down * armLowerDistance;
        }

        while (elapsed < downDuration)
        {
            float t = elapsed / downDuration;
            for (int i = 0; i < leftArms.Length; i++)
            {
                leftArms[i].localPosition = Vector3.Lerp(leftStart[i], leftDownTarget[i], t);
                rightArms[i].localPosition = Vector3.Lerp(rightStart[i], rightDownTarget[i], t);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < leftArms.Length; i++)
        {
            leftArms[i].localPosition = leftDownTarget[i];
            rightArms[i].localPosition = rightDownTarget[i];
        }

        // Move the claw arms inward to grip
        float inwardDuration = baseClawDuration / clawSpeedMult;
        elapsed = 0f;

        Vector3[] leftGripTarget = new Vector3[leftArms.Length];
        Vector3[] rightGripTarget = new Vector3[rightArms.Length];

        for (int i = 0; i < leftArms.Length; i++)
        {
            leftGripTarget[i] = leftDownTarget[i] + Vector3.right * armGripDistance;
            rightGripTarget[i] = rightDownTarget[i] + Vector3.left * armGripDistance;

            leftStart[i] = leftArms[i].localPosition;
            rightStart[i] = rightArms[i].localPosition;
        }

        while (elapsed < inwardDuration)
        {
            float t = elapsed / inwardDuration;
            for (int i = 0; i < leftArms.Length; i++)
            {
                leftArms[i].localPosition = Vector3.Lerp(leftStart[i], leftGripTarget[i], t);
                rightArms[i].localPosition = Vector3.Lerp(rightStart[i], rightGripTarget[i], t);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < leftArms.Length; i++)
        {
            leftArms[i].localPosition = leftGripTarget[i];
            rightArms[i].localPosition = rightGripTarget[i];
        }
    }

    private IEnumerator LiftCup(Transform snapTransform)
    {
        float duration = baseClawDuration / clawSpeedMult;
        float elapsed = 0f;

        Vector3 cupStart = snapTransform.position;
        Vector3 cupTarget = cupStart + Vector3.up * liftHeight;

        Vector3[] leftStart = new Vector3[leftArms.Length];
        Vector3[] rightStart = new Vector3[rightArms.Length];

        Vector3[] leftTarget = new Vector3[leftArms.Length];
        Vector3[] rightTarget = new Vector3[rightArms.Length];

        for (int i = 0; i < leftArms.Length; i++)
        {
            leftStart[i] = leftArms[i].localPosition;
            rightStart[i] = rightArms[i].localPosition;

            leftTarget[i] = leftStart[i] + Vector3.up * liftHeight;
            rightTarget[i] = rightStart[i] + Vector3.up * liftHeight;
        }

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            snapTransform.position = Vector3.Lerp(cupStart, cupTarget, t);

            for (int i = 0; i < leftArms.Length; i++)
            {
                leftArms[i].localPosition = Vector3.Lerp(leftStart[i], leftTarget[i], t);
                rightArms[i].localPosition = Vector3.Lerp(rightStart[i], rightTarget[i], t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        snapTransform.position = cupTarget;

        for (int i = 0; i < leftArms.Length; i++)
        {
            leftArms[i].localPosition = leftTarget[i];
            rightArms[i].localPosition = rightTarget[i];
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
        float duration = baseClawDuration / clawSpeedMult;
        float elapsed = 0f;

        Vector3 cupStart = snapTransform.position;
        Vector3 cupTarget = cupStart - Vector3.up * liftHeight;

        Vector3[] leftStart = new Vector3[leftArms.Length];
        Vector3[] rightStart = new Vector3[rightArms.Length];

        Vector3[] leftTarget = new Vector3[leftArms.Length];
        Vector3[] rightTarget = new Vector3[rightArms.Length];

        for (int i = 0; i < leftArms.Length; i++)
        {
            leftStart[i] = leftArms[i].localPosition;
            rightStart[i] = rightArms[i].localPosition;

            leftTarget[i] = leftStart[i] - Vector3.up * liftHeight;
            rightTarget[i] = rightStart[i] - Vector3.up * liftHeight;
        }

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            snapTransform.position = Vector3.Lerp(cupStart, cupTarget, t);

            for (int i = 0; i < leftArms.Length; i++)
            {
                leftArms[i].localPosition = Vector3.Lerp(leftStart[i], leftTarget[i], t);
                rightArms[i].localPosition = Vector3.Lerp(rightStart[i], rightTarget[i], t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        snapTransform.position = cupTarget;

        for (int i = 0; i < leftArms.Length; i++)
        {
            leftArms[i].localPosition = leftTarget[i];
            rightArms[i].localPosition = rightTarget[i];
        }
    }

    private IEnumerator ReleaseCup(Transform snapTransform, Cup cup)
    {
        float outwardDuration = (baseClawDuration / 2f) / clawSpeedMult;

        Vector3[] leftStart = new Vector3[leftArms.Length];
        Vector3[] rightStart = new Vector3[rightArms.Length];

        Vector3[] leftOutTarget = new Vector3[leftArms.Length];
        Vector3[] rightOutTarget = new Vector3[rightArms.Length];

        for (int i = 0; i < leftArms.Length; i++)
        {
            leftStart[i] = leftArms[i].localPosition;
            rightStart[i] = rightArms[i].localPosition;

            leftOutTarget[i] = leftStart[i] + Vector3.left * armGripDistance;
            rightOutTarget[i] = rightStart[i] + Vector3.right * armGripDistance;
        }

        float elapsed = 0f;
        while (elapsed < outwardDuration)
        {
            float t = elapsed / outwardDuration;

            for (int i = 0; i < leftArms.Length; i++)
            {
                leftArms[i].localPosition = Vector3.Lerp(leftStart[i], leftOutTarget[i], t);
                rightArms[i].localPosition = Vector3.Lerp(rightStart[i], rightOutTarget[i], t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < leftArms.Length; i++)
        {
            leftArms[i].localPosition = leftOutTarget[i];
            rightArms[i].localPosition = rightOutTarget[i];
        }

        cup.SetGrabEnabled(true);

        float upDuration = baseClawDuration / clawSpeedMult;

        for (int i = 0; i < leftArms.Length; i++)
        {
            leftStart[i] = leftArms[i].localPosition;
            rightStart[i] = rightArms[i].localPosition;
        }

        elapsed = 0f;
        while (elapsed < upDuration)
        {
            float t = elapsed / upDuration;

            for (int i = 0; i < leftArms.Length; i++)
            {
                leftArms[i].localPosition = Vector3.Lerp(leftStart[i], leftRestPositions[i], t);
                rightArms[i].localPosition = Vector3.Lerp(rightStart[i], rightRestPositions[i], t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < leftArms.Length; i++)
        {
            leftArms[i].localPosition = leftRestPositions[i];
            rightArms[i].localPosition = rightRestPositions[i];
        }

    }

    /*----------------Employee Interaction-------------------*/
    public override void OnCupInserted(Cup cup)
    {
        base.OnCupInserted(cup);

        // New cup inserted, allow employee to work
        isBeingWorked = false;
    }

    public override bool CanEmployeeWork()
    {
        return HasAnyCup() && !CheckCupCompletion() && !isBeingWorked;
    }

    protected override IEnumerator EmployeeWorkLoop(Employee employee)
    {
        MachineRipcord ripcord = trigger as MachineRipcord;
        if (ripcord == null || employee == null)
            yield break;

        while (employee.CurrentMachine == this)
        {
            if (CanEmployeeWork())
            {
                isBeingWorked = true;

                // Chance to fail based on fatigue
                int fatigue = employee.GetFatigueLevel();
                float activationChance = fatigue switch
                {
                    0 => 1f,
                    1 => 0.85f,
                    2 => 0.65f,
                    3 => 0.5f,
                    4 => 0.3f,
                    _ => 1f
                };

                bool success = Random.value <= activationChance;

                if (success)
                {
                    ripcord.RemoteActivate(1f);
                    employee.OnCupCompleted();
                    StopEmployeeWork();
                }
                else
                {
                    // Failed pull animation + short delay
                    yield return ripcord.PlayFailedPullAnimation();
                    yield return new WaitForSeconds(2f);
                    isBeingWorked = false;
                }
            }

            yield return null; 
        }

        StopEmployeeWork();
    }
}
