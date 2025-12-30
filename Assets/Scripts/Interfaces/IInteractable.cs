using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerControls player);
    void OnRelease(Vector3 releasePos);
    void OnHold();
}
