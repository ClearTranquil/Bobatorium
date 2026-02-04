using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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
    private bool isWaitingToAct = false;
    [SerializeField] private float workSpeed = 1;

    private void Awake()
    {
        mainCam = Camera.main;
        originalLayer = gameObject.layer;
    }

    /*--------------------INTERACTABLE--------------------*/

    // No current reason employees should be locked out of interaction
    public bool CanInteract(PlayerControls player) => true;

    // Employees can be dragged onto machines to assign them to it
    public void Interact(PlayerControls player)
    {
        // Pick up employee, tell machine they aren't assigned anymore
        if (currentSnapPoint != null)
        {
            currentSnapPoint.Clear();
            currentSnapPoint = null;
        }

        transform.SetParent(null);
        gameObject.layer = heldLayer;

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

        if (heldSnapPoint != null)
            desiredPos = heldSnapPoint.transform.position;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, 0.05f);
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
    }

    public void ClearSnapPoint()
    {
        currentSnapPoint = null;
        CurrentMachine = null;
    }

    /*--------------Machine Interaction------------*/
    // Employees wait for a cup to be put in the machine before operating it
    public void OnMachineCupInserted()
    {
        if (CurrentMachine == null)
            return;

        if (isWaitingToAct)
            return;

        var trigger = CurrentMachine.GetTrigger();
        if (trigger == null)
            return;

        StartCoroutine(OperateMachine(trigger));
    }

    public void AssignMachine(Machine m_machine)
    {
        CurrentMachine = m_machine;
    }

    IEnumerator OperateMachine(MachineTriggerBase trigger)
    {
        isWaitingToAct = true;

        yield return new WaitForSeconds(reactionDelay);

        yield return StartCoroutine(OperateTrigger(trigger));

        isWaitingToAct = false;
    }

    IEnumerator OperateTrigger(MachineTriggerBase trigger)
    {
        trigger.BeginRemoteHold();

        float duration = trigger.GetHoldDuration(workSpeed);
        float t = 0f;

        while (t < duration)
        {
            trigger.TickRemoteHold(Time.deltaTime, workSpeed);
            t += Time.deltaTime;
            yield return null;
        }

        trigger.EndRemoteHold();
    }
}
