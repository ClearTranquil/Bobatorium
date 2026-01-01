using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MachineLever : MachineTriggerBase
{
    [Header("Lever options")]
    [SerializeField] private float maxPullDistance = 1.0f;
    [SerializeField] private float pullSensitivity = 4f;
    [SerializeField] private float returnSpeed = 6f;
    [SerializeField] private float triggerThreshold = 0.9f;

    [SerializeField] private GameObject handle;
    private Vector3 startPos;
    private float currentPullAmount;

    private void Awake()
    {
        startPos = handle.gameObject.transform.localPosition;
    }

    private void Update()
    {
        LeverMovement();
        HandleMachineTrigger();
    }
    
    private void LeverMovement()
    {
        if (isHeld && Mouse.current.leftButton.isPressed)
        {
            // Checks how much the mouse has moved up or down while the lever is held and moves the lever accordingly
            float mouseDeltaY = Mouse.current.delta.ReadValue().y;

            currentPullAmount -= mouseDeltaY * Time.deltaTime * pullSensitivity;
            currentPullAmount = Mathf.Clamp01(currentPullAmount);
        }
        else
        {
            // Returns to neutral position
            currentPullAmount = Mathf.MoveTowards(currentPullAmount, 0, returnSpeed);
        }

        handle.transform.localPosition = startPos + Vector3.down * currentPullAmount * maxPullDistance;
    }

    private void HandleMachineTrigger()
    {
        // When lever is pulled down long enough, trigger the machine
        if (currentPullAmount >= triggerThreshold && isHeld)
        {
            TriggerMachine();
        }
    }
}
