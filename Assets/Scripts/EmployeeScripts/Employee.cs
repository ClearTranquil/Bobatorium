using UnityEngine;
using UnityEngine.InputSystem;

public class Employee : MonoBehaviour, IInteractable
{
    private Camera mainCam;
    private Vector3 velocity;
    private Vector3 desiredPos;

    private EmployeeSnapPoint heldSnapPoint;
    private EmployeeSnapPoint currentSnapPoint;

    [Header("Physics")]
    [SerializeField] private float heldZDistance = 40f;

    [Header("Position Snapping")]
    [SerializeField] private LayerMask snapMask;
    [SerializeField] private float snapMaxDistance = 100f;
    private int originalLayer;
    private int heldLayer = 7;

    private void Awake()
    {
        mainCam = Camera.main;
        originalLayer = gameObject.layer;
    }

    /*--------------------INTERACTABLE--------------------*/

    public bool CanInteract(PlayerControls player) => true;

    public void Interact(PlayerControls player)
    {
        // Pick up employee
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

        // Check for snap points
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

        // Only standalone snap points exist
        var snapPoint = hit.collider.GetComponentInParent<EmployeeSnapPoint>();
        if (snapPoint != null && !snapPoint.IsOccupied)
            return snapPoint;

        return null;
    }

    public void RegisterSnapPoint(EmployeeSnapPoint snapPoint)
    {
        currentSnapPoint = snapPoint;
    }

    public void ClearSnapPoint()
    {
        currentSnapPoint = null;
    }
}
