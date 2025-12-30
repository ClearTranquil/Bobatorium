using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerControls player);

    // Optional, only called if object can be held
    void OnRelease(Vector3 releasePos);
}
