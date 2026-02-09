using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MachineRipcord : MachineTriggerBase
{
    [SerializeField] private GameObject handle;
    private LineRenderer line;
    [SerializeField] private Transform lineStart;
    public override bool CanRepeat => false;

    [SerializeField] private float maxPullDistance = 1f;
    [SerializeField] private float minPullSpeed = 4f;
    [SerializeField] private float retractSpeed = 2f;

    private Vector3 startPos;
    private Vector3 lastPos;
    private float currentVelocity;


    private void Awake()
    {
        startPos = handle.transform.position;
        lastPos = startPos;

        line = GetComponent<LineRenderer>();
    }

    private void LateUpdate()
    {
        // Adds a cable between the base and the handle
        if (!line) 
            return;

        line.SetPosition(0, lineStart.position);
        line.SetPosition(1, handle.transform.position);
    }

    public override void Interact(PlayerControls player)
    {
        base.Interact(player);
        player.PickUp(gameObject);

        isHeld = true;
        lastPos = handle.transform.position;
    }

    /* The player has to pull the handle until it reaches max distance quickly enough for it to trigger.
       if not fast enough, machine wont trigger and the ripcord retracts.*/
    public override void OnHold()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, Camera.main.WorldToScreenPoint(startPos).z));

        Vector3 pullVector = mouseWorld - startPos;
        Vector3 clamped = Vector3.ClampMagnitude(pullVector, maxPullDistance);

        handle.transform.position = startPos + clamped;

        // Measure handle's velocity
        currentVelocity = (handle.transform.position - lastPos).magnitude / Time.deltaTime;
        lastPos = handle.transform.position;

        // Trigger only if pulled fast AND reaches max
        if (clamped.magnitude >= maxPullDistance)
        {
            if (currentVelocity >= minPullSpeed)
            {
                TriggerMachine();
            } else
            {
                //PlayerControls.TryForceRelease();
                //isHeld = false;
            }
        }
    }

    public override void OnRelease(Vector3 releasePos)
    {
        // Retract when not held
        isHeld = false;
    }

    private void Update()
    {
        if (!isHeld)
        {
            handle.transform.position = Vector3.MoveTowards(handle.transform.position, startPos, retractSpeed * Time.deltaTime);
        }
    }

/*------------Employee Interaction---------------*/
    public override void RemoteActivate(float workSpeed)
    {
        handle.transform.position = startPos + Vector3.down * maxPullDistance;
        TriggerMachine();
    }

    public IEnumerator PlayFailedPullAnimation()
    {
        isHeld = true;
        handle.transform.position = startPos;
        yield return FailedPullRoutine();
    }

    // Plays an animation where the employee fails to pull the ripcord fast enough
    private IEnumerator FailedPullRoutine()
    {
        float failSpeed = 7f;
        Vector3 targetPos = startPos + Vector3.down * maxPullDistance;

        while ((handle.transform.position - targetPos).sqrMagnitude > 0.001f)
        {
            handle.transform.position = Vector3.MoveTowards(handle.transform.position, targetPos, failSpeed * Time.deltaTime);
            yield return null;
        }

        // Allow the cord to retract
        isHeld = false;
    }
}
