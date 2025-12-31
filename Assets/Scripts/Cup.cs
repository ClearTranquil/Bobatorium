using UnityEngine;
using UnityEngine.InputSystem;

public class Cup : MonoBehaviour, IInteractable
{
    private int bobaCount = 0;
    private float teaFill;
    private bool isSealed;

    [SerializeField] private float heldZDistance = 15f;

    private SnapPoints heldSnapPoint;
    private SnapPoints currentSnapPoint;
    [SerializeField] private LayerMask snapMask;
    [SerializeField] private float snapMaxDistance = 100f;

    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private int GetBobaCount()
        { return bobaCount; }

    private float GetTeaFill()
        { return teaFill; }

    private bool GetIsSealed()
        { return isSealed; }

    public void Interact(PlayerControls player)
    {
        // If picked back up while it was snapped to a point, tell the snapPoint its been removed
        if(currentSnapPoint != null)
        {
            currentSnapPoint.Clear();
            ClearSnapPoint();
        }
        
        transform.SetParent(null);
        player.PickUp(gameObject);
    }


    public void OnRelease(Vector3 releasePos)
    {
        //Debug.Log("Cup released");

        // Check if the cup should be snapping to a nearby snap point 
        if (heldSnapPoint != null)
        {
            heldSnapPoint.TrySnapCup(this);
            currentSnapPoint = heldSnapPoint;
            heldSnapPoint = null;
            return;
        }

        // Logic for cups not dropped in machine here
    }

    public void OnHold()
    {
        // While held, cup follows the player's cursor by default
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector3 targetPos = mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, heldZDistance));
        transform.position = targetPos;

        // While held, the cup looks for snap points. If one is nearby, snap to it to "preview" where it will be placed if the player lets go. 
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        // First, check if the obj being hovered over is a machine. Machine snap point behavior checks all available snap points and puts the cup in the first available one.
        if (Physics.Raycast(ray, out RaycastHit hit, snapMaxDistance))
        {
            Machine machine = hit.collider.GetComponent<Machine>();
            if (machine != null)
            {
                SnapPoints snap = machine.GetAvailableSnapPoint();
                if (snap != null)
                {
                    heldSnapPoint = snap;
                    transform.position = snap.transform.position;
                    return;
                }
            }
        }

        // Second, check for direct snap points that arent tied to a machine. 
        if (Physics.Raycast(ray, out RaycastHit snapHit, snapMaxDistance, snapMask))
        {
            SnapPoints snap = snapHit.collider.GetComponent<SnapPoints>();
            if (snap != null && !snap.IsOccupied)
            {
                heldSnapPoint = snap;
                transform.position = snap.transform.position;
                return;
            }
        }
        
        // Third, if theres no snap points nearby, just keep the object glued to the cursor.
        heldSnapPoint = null;
    }

    public void RegisterSnapPoint(SnapPoints snapPoint)
    {
        currentSnapPoint = snapPoint;
    }

    public void ClearSnapPoint()
    {
        currentSnapPoint = null;
    }
}
