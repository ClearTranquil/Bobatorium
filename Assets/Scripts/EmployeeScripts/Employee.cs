using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class Employee : MonoBehaviour, IInteractable
{
    private Camera mainCam;
    private Vector3 velocity;
    private Vector3 desiredPos;

    [Header("Physics")]
    [SerializeField] private float heldZDistance = 40f;

    [Header("Position Snapping")]
    [SerializeField] private LayerMask snapMask;
    [SerializeField] private float snapMaxDistance = 100f;
    private int originalLayer;
    private int heldLayer = 7;
    private EmployeeSnapPoint heldSnapPoint;
    private EmployeeSnapPoint currentSnapPoint;

    [Header("Machine Interaction")]
    public Machine CurrentMachine { get; private set; }
    [SerializeField] private float reactionDelay = 0.5f;
    [SerializeField] private float workSpeed = 1;
    private Coroutine workLoop;

    [Header("Fatigue levels")]
    [SerializeField] private int fatigueLevel = 0;
    [SerializeField] private int maxFatigue = 4;

    [Header("Fatigue Visuals")]
    [SerializeField] private SpriteRenderer faceRenderer;
    [SerializeField] private Sprite[] fatigueFaceSprites;

    [Header("Visual Rotation")]
    [SerializeField] private Transform modelTransform;
    [SerializeField] private float rotationSpeed = 360f;
    private Quaternion targetModelRotation;

    // Each employee has random fatigue thresholds
    private int cupsUntilCheck;
    [SerializeField] private int fatigueChanceDenominator = 8;

    private int cupsCompleted;
    private bool isAsleep;

    private void Awake()
    {
        mainCam = Camera.main;
        originalLayer = gameObject.layer;

        InitializeFatigue();
    }

    private void Update()
    {
        if (!modelTransform) return;

        modelTransform.rotation = Quaternion.RotateTowards(modelTransform.rotation, targetModelRotation, rotationSpeed * Time.deltaTime);
    }

    private void InitializeFatigue()
    {
        cupsUntilCheck = Random.Range(2, 9);
        UpdateFatigueVisuals();
    }

    /*--------------------INTERACTABLE--------------------*/

    // No current reason employees should be locked out of interaction
    public bool CanInteract(PlayerControls player) => true;

    // Employees can be dragged onto machines to assign them to it
    public void Interact(PlayerControls player)
    {
        // If the employee is asleep, wake them up instead of any other interaction stuff
        if (isAsleep)
        {
            WakeUp();
            return;
        }

        // Pick up employee, tell machine they aren't assigned anymore
        if (currentSnapPoint != null)
        {
            CurrentMachine.RemoveActiveEmployee(this);
            currentSnapPoint.Clear();
            currentSnapPoint = null;
            CurrentMachine = null;

            StopWorkLoop();
        }

        transform.SetParent(null);
        gameObject.layer = heldLayer;

        FaceCamera();

        player.PickUp(gameObject);
    }

    public void OnRelease(Vector3 releasePos)
    {
        // Reset layer
        gameObject.layer = originalLayer;

        // Snap to point if available
        if (heldSnapPoint != null)
        {
            if (heldSnapPoint.TrySnap(this))
            {
                currentSnapPoint = heldSnapPoint;
                transform.position = currentSnapPoint.transform.position;
                velocity = Vector3.zero;
            }
            heldSnapPoint = null;
        }
    }

    public void OnHold()
    {
        // Follow mouse
        Vector3 mousePos = Mouse.current.position.ReadValue();
        desiredPos = mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, heldZDistance));

        // Check for employee snap points
        Ray ray = mainCam.ScreenPointToRay(mousePos);
        heldSnapPoint = FindBestSnapPoint(ray);

        if (heldSnapPoint)
        {
            desiredPos = heldSnapPoint.transform.position;
            targetModelRotation = heldSnapPoint.transform.rotation;
        } else
        {
            FaceCamera();
        }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, 0.05f);
    }

    private void FaceCamera()
    {
        if (!modelTransform || !mainCam) return;

        Vector3 direction = mainCam.transform.forward;
        direction.y = 0f;

        targetModelRotation = Quaternion.LookRotation(direction);
    }


    /*--------------------SNAP LOGIC--------------------*/


    private EmployeeSnapPoint FindBestSnapPoint(Ray ray)
    {
        if (!Physics.Raycast(ray, out RaycastHit hit, snapMaxDistance, snapMask))
            return null;

        var snapPoint = hit.collider.GetComponentInParent<EmployeeSnapPoint>();
        if (snapPoint != null && !snapPoint.IsOccupied)
            return snapPoint;

        return null;
    }

    // Tell machine an employee is assigned to it
    public void RegisterSnapPoint(EmployeeSnapPoint snapPoint)
    {
        currentSnapPoint = snapPoint;
        CurrentMachine = snapPoint.GetComponentInParent<Machine>();

        CurrentMachine.SetActiveEmployee(this);

        snapPoint.OnEmployeePlaced();
        targetModelRotation = snapPoint.transform.rotation;
        StartWorkLoop();
    }

    /*--------------Machine Interaction------------*/

    private void StartWorkLoop()
    {
        if (workLoop != null)
            return;

        workLoop = StartCoroutine(EmployeeWorkLoop());
    }

    private void StopWorkLoop()
    {
        if (workLoop != null)
        {
            StopCoroutine(workLoop);
            workLoop = null;
        }
    }

    private IEnumerator EmployeeWorkLoop()
    {
        while (true)
        {
            if (!CurrentMachine || isAsleep)
            {
                yield return null;
                continue;
            }

            if (CurrentMachine.CanEmployeeWork())
            {
                CurrentMachine.ActivateByEmployee(GetEffectiveWorkSpeed());
            }

            yield return new WaitForSeconds(reactionDelay);

        }
    }

    public void AssignMachine(Machine m_machine)
    {
        CurrentMachine = m_machine;
    }

    public void OnCupCompleted()
    {
        Debug.Log("Cup complete");
        cupsCompleted++;

        if (cupsCompleted < cupsUntilCheck)
            return;

        cupsCompleted = 0;

        TryIncreaseFatigue();
    }

    public float GetEffectiveWorkSpeed()
    {
        if (isAsleep)
            return 0f;

        float fatigueMultiplier = 1f - (fatigueLevel * 0.15f);
        fatigueMultiplier = Mathf.Clamp(fatigueMultiplier, 0.25f, 1f);

        return workSpeed * fatigueMultiplier;
    }

    /*---------Fatigue-----------*/
    private void TryIncreaseFatigue()
    {
        if (isAsleep)
            return;

        int roll = Random.Range(1, fatigueChanceDenominator + 1);
        if (roll == 1)
        {
            IncreaseFatigue(1);
        }
    }

    private void IncreaseFatigue(int amount)
    {
        fatigueLevel = Mathf.Clamp(fatigueLevel + amount, 0, maxFatigue);

        UpdateFatigueVisuals();

        if (fatigueLevel >= maxFatigue)
        {
            PassOut();
        }
    }

    public void HealFatigue(int amount)
    {
        if (fatigueLevel == 0) return;
        fatigueLevel = Mathf.Clamp(fatigueLevel - amount, 0, maxFatigue);

        UpdateFatigueVisuals();
    }

    public void ResetFatigue()
    {
        fatigueLevel = 0;
        isAsleep = false;
        cupsCompleted = 0;

        UpdateFatigueVisuals();
    }

    public int GetFatigueLevel()
    {
        return fatigueLevel;
    }

    private void PassOut()
    {
        isAsleep = true;

        // Stop machine immediately
        if (CurrentMachine)
            CurrentMachine.StopEmployeeWork();

        // Play "head hitting machine" sound effect 
        // Maybe play snoring sound effect
        // Maybe display a "ZZZ..." vfx

        UpdateFatigueVisuals();
    }

    private void WakeUp()
    {
        // Play slap sound
        // Play "waking up" animation?

        isAsleep = false;
        fatigueLevel = 3; // NOT fully rested

        UpdateFatigueVisuals();
    }

    private void UpdateFatigueVisuals()
    {
        if (!faceRenderer || fatigueFaceSprites == null || fatigueFaceSprites.Length == 0)
            return;

        int spriteIndex = Mathf.Clamp(fatigueLevel, 0, fatigueFaceSprites.Length - 1);
        faceRenderer.sprite = fatigueFaceSprites[spriteIndex];
    }

    /*-------Buffs---------*/
    public void ModifyWorkSpeed(float multiplier, float duration)
    {
        StartCoroutine(TemporaryWorkSpeedModifier(multiplier, duration));
    }

    private IEnumerator TemporaryWorkSpeedModifier(float multiplier, float duration)
    {
        // Using a syringe on the employee makes then work much faster for a time, then increases fatigue when it wears off
        workSpeed *= multiplier;
        yield return new WaitForSeconds(duration);
        workSpeed /= multiplier;

        IncreaseFatigue(1);
    }
}
