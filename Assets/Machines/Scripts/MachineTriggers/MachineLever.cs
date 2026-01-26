using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MachineLever : MachineTriggerBase
{
    [Header("Lever options")]
    [SerializeField] private float maxPullAngle = 60f;
    [SerializeField] private float pullSensitivity = 4f;
    [SerializeField] private float returnSpeed = 6f;
    [SerializeField] private float triggerThreshold = 45f;

    [SerializeField] private GameObject handle;
    private float currentAngle;
    private float startAngle;

    private void Awake()
    {
        startAngle = handle.transform.localEulerAngles.z;

        if (startAngle > 180f)
            startAngle -= 360f;

        currentAngle = 0f;
    }

    public override void Interact(PlayerControls player)
    {
        base.Interact(player);
        player.PickUp(gameObject);
        isHeld = true;
    }

    private void Update()
    {
        LeverMovement();
        HandleMachineTrigger();
    }
    
    private void LeverMovement()
    {
        // Returns to neutral position when not held
        if (!isHeld && currentAngle > 0f)
        {
            currentAngle = Mathf.MoveTowards(currentAngle, 0f, returnSpeed * Time.deltaTime);
        }

        ApplyRotation();
    }

    public override void OnHold()
    {
        float mouseDeltaY = Mouse.current.delta.ReadValue().y;

        // Pull down when dragging mouse downward
        currentAngle += -mouseDeltaY * pullSensitivity;
        currentAngle = Mathf.Clamp(currentAngle, 0f, maxPullAngle);

        ApplyRotation();
    }

    public override void OnRelease(Vector3 releasePos)
    {
        isHeld = false;
    }

    // Visually moves the handle, doesn't effect the logic
    private void ApplyRotation()
    {
        handle.transform.localRotation = (Quaternion.Euler(0f, 0f, startAngle + currentAngle));
    }

    private void HandleMachineTrigger()
    {
        // When lever is pulled down long enough, trigger the machine
        if (currentAngle >= triggerThreshold && isHeld)
        {
            TriggerMachine();
        } else
        {
            StopTriggerMachine();
        }
    }
}
