using UnityEngine;
using UnityEngine.InputSystem;

public class MachineAnalogLever : MachineTriggerBase
{
    // This is a clone of the regular lever that works with machines that accept a trigger value between 0-1
    [Header("Lever Options")]
    [SerializeField] private float maxPullAngle = 60f;
    [SerializeField] private float pullSensitivity = 4f;

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
        if (isOperating) return;

        base.Interact(player);
        player.PickUp(gameObject);
        isHeld = true;
    }

    private void Update()
    {
        ApplyRotation();
        SendTriggerStrength();
    }

    public override void OnHold()
    {
        if (isOperating) return;

        float mouseDeltaX = -Mouse.current.delta.ReadValue().x;

        currentAngle += -mouseDeltaX * pullSensitivity;
        currentAngle = Mathf.Clamp(currentAngle, 0f, maxPullAngle);

        ApplyRotation();
    }

    public override void OnRelease(Vector3 releasePos)
    {
        if (isOperating) return;

        isHeld = false;
    }

    private void ApplyRotation()
    {
        handle.transform.localRotation = Quaternion.Euler(0f, 0f, startAngle + currentAngle);
    }

    private void SendTriggerStrength()
    {
        if (!machine) return;

        float normalized = currentAngle / maxPullAngle;
        machine.SetTriggerStrength(normalized);
    }
}
