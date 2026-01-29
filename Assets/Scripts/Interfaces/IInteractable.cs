using UnityEngine;

public interface IInteractable
{
    bool CanInteract(PlayerControls player);
    void Interact(PlayerControls player);
    void OnRelease(Vector3 releasePos);
    void OnRightClick(PlayerControls player) { }
    void OnHold();
}
