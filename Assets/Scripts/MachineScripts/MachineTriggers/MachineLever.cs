using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class MachineLever : MachineTriggerBase
{
    [Header("Lever options")]
    [SerializeField] private float maxPullAngle = 60f;
    [SerializeField] private float pullSensitivity = 4f;
    [SerializeField] private float returnSpeed = 6f;
    [SerializeField] private float triggerThreshold = 45f;

    [Header("Employee options")]
    [SerializeField] private float baseEmployeePullSpeed = 120f;

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
        // Player can't mess with lever if the employee is using it
        if (isOperating) return;

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
        if (!isHeld && !isOperating && currentAngle > 0f)
        {
            currentAngle = Mathf.MoveTowards(currentAngle, 0f, returnSpeed * Time.deltaTime);
        }

        ApplyRotation();
    }

    public override void OnHold()
    {
        // Player can't mess with lever if the employee is using it
        if (isOperating) return;

        float mouseDeltaY = Mouse.current.delta.ReadValue().y;

        // Pull down when dragging mouse downward
        currentAngle += -mouseDeltaY * pullSensitivity;
        currentAngle = Mathf.Clamp(currentAngle, 0f, maxPullAngle);

        ApplyRotation();
    }

    public override void OnRelease(Vector3 releasePos)
    {
        // Player can't mess with lever if the employee is using it
        if (isOperating) return;

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

    // Employee interaction
    public override void RemoteActivate(float workSpeed)
    {
        if (isOperating)
            return;

        StartCoroutine(PullLeverRoutine(workSpeed));
    }

    public override void StopOperating()
    {
        base.StopOperating();
        isHeld = false;
    }

    private IEnumerator PullLeverRoutine(float workSpeed)
    {
        isOperating = true;
        isHeld = true;

        // Pull lever down until threshold
        while (currentAngle < triggerThreshold)
        {
            currentAngle += baseEmployeePullSpeed * workSpeed * Time.deltaTime; // use basePullSpeed
            currentAngle = Mathf.Clamp(currentAngle, 0f, maxPullAngle);
            ApplyRotation();
            yield return null;
        }

        TriggerMachine();

        // Hold at threshold until stopped
        while (isOperating)
        {
            currentAngle = triggerThreshold;
            ApplyRotation();
            yield return null;
        }

        // Return to neutral
        while (currentAngle > 0f)
        {
            currentAngle = Mathf.MoveTowards(currentAngle, 0f, returnSpeed * Time.deltaTime);
            ApplyRotation();
            yield return null;
        }

        isHeld = false;
    }
}
