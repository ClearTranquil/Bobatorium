using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    private GameObject heldObj;

    [SerializeField] private LayerMask leftClickMask; // for things up, using triggers
    [SerializeField] private LayerMask rightClickMask; // for upgrading things

    public static event Action ForceRelease;

    /* The player controls script only handles the act of clicking and dragging things. It has no idea what it's actually clicking on or interacting with. 
     This is by design! Interactable objects use an interface that decides what happens when they're clicked/dragged by player controls. */

    private void Update()
    {
        HandleInteraction();
        HandleRightClick();
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
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, leftClickMask))
        {
            //Debug.Log("Clicked on " + hit.collider.gameObject);
            
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (!interactable.CanInteract(this))
                return;

            interactable.Interact(this);
        }
    }

    private void HandleRightClick()
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, rightClickMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract(this))
            {
                interactable.OnRightClick(this);
            }
        }
    }

    private void HandleDrag()
    {
        if (heldObj == null || !Mouse.current.leftButton.isPressed)
            return;

        IInteractable interactable = heldObj.GetComponent<IInteractable>();
        interactable?.OnHold();
    }

    private void HandleRelease()
    {
        if (heldObj == null || !Mouse.current.leftButton.wasReleasedThisFrame) 
            return;

        // Tell the held object it was released
        var interactable = heldObj.GetComponent<IInteractable>();
        interactable?.OnRelease(heldObj.transform.position);

        heldObj = null;
    }

    // Called by interactables
    public void PickUp(GameObject obj)
    {
        heldObj = obj;
    }

    public static void TryForceRelease()
    {
        ForceRelease?.Invoke();
    }

    private void OnEnable()
    {
        ForceRelease += HandleForceRelease;
    }

    private void HandleForceRelease()
    {
        var interactable = heldObj.GetComponent<IInteractable>();
        interactable?.OnRelease(heldObj.transform.position);

        heldObj = null;
    }
}
