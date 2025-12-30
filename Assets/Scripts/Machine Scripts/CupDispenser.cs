using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Burst.Intrinsics.X86.Avx;

public class CupDispenser : MonoBehaviour, IInteractable
{
    [SerializeField] private Cup cupPrefab;
    [SerializeField] private float spawnOffsetY = 1f;

    // Spawns a cup at the player's hand 
    public void Interact(PlayerControls player)
    {
        Vector3 spawnPos = transform.position + Vector3.up * spawnOffsetY;
        Cup cup = Instantiate(cupPrefab, spawnPos, Quaternion.identity);

        player.PickUp(cup.gameObject);
    }

    // Doesn't need an OnRelease case just yet
    public void OnRelease(Vector3 releasePos) { }
}
