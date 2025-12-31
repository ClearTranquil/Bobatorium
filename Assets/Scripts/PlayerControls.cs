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

        if (heldObj)
        {
            var interactable = heldObj.GetComponent<IInteractable>();
            interactable?.OnHold();
        }
    }

    private void HandleInteraction()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        // If object is Interactable, tell it that its being interacted with
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, rayCastMask))
        {
            Debug.Log("Clicked on " + hit.collider.gameObject);
            
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

        // Tell object its being held
        IInteractable interactable = heldObj.GetComponent<IInteractable>();
        interactable?.OnHold();

        heldObj.transform.position = targetPos;
    }

    private void HandleRelease()
    {
        if (heldObj == null || !Mouse.current.leftButton.wasReleasedThisFrame) 
            return;

        // Tell the held object it was released
        heldObj.layer = originalLayer;
        var interactable = heldObj.GetComponent<IInteractable>();
        interactable?.OnRelease(heldObj.transform.position);

        heldObj = null;
    }

    // Called by interactables
    public void PickUp(GameObject obj)
    {
        heldObj = obj;

        // Update's held objects layer so it doesn't block raycasts
        originalLayer = heldObj.layer;
        heldObj.layer = heldObjLayer;
    }
}
