using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    private GameObject heldObj;
    [SerializeField] private float heldZDistance = 8f;

    [SerializeField] private LayerMask rayCastMask;
    private int originalLayer;
    private int heldObjLayer = 7;

    /* The player controls script only handles the act of clicking and dragging things. It has no idea what it's actually clicking on or interacting with. 
     This is by design! Interactable objects use an interface that decides what happens when they're clicked/dragged by player controls. */

    private void Update()
    {
        HandleInteraction();
        HandleDrag();
        HandleRelease();
    }

    private void HandleInteraction()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        // If object is Interactable, tell it that its being interacted with
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, rayCastMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            interactable?.Interact(this);
        }
    }

    private void HandleDrag()
    {
        if (heldObj == null)
            return;

        if (!Mouse.current.leftButton.isPressed)
            return;

        // Holds an object a fixed distance from the camera. This may change later. 
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 targetPos = ray.GetPoint(heldZDistance);

        // Machines/delivery trays have a set position that held cups will "snap" to if held near the machine.
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, rayCastMask))
        {
            Machine machine = hit.collider.GetComponent<Machine>();
            if (machine != null && machine.snapPoint != null)
            {
                targetPos = machine.snapPoint.position;
            }
        }

        heldObj.transform.position = targetPos;
    }

    private void HandleRelease()
    {
        if (heldObj == null)
            return;

        // If object is interactable, tell it that the player just let go of it 
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            IInteractable interactable = heldObj.GetComponent<IInteractable>();
            interactable?.OnRelease();

            // Reset object's layer so it can be targeted again
            heldObj.layer = originalLayer;

            heldObj = null;
        }
    }

    // Called by interactables
    public void PickUp(GameObject obj)
    {
        heldObj = obj;

        // Update's held objects layer so it doesn't block raycasts d
        originalLayer = heldObj.layer;
        heldObj.layer = heldObjLayer;
    }
}
