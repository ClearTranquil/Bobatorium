using UnityEngine;

public class CupDispenser : MonoBehaviour, IInteractable
{
    [SerializeField] private Cup cupPrefab;
    [SerializeField] private float spawnOffsetY = 1f;

    public bool CanInteract(PlayerControls player)
    {
        return true;
    }

    // Spawns a cup at the player's hand 
    public void Interact(PlayerControls player)
    {
        Vector3 spawnPos = transform.position + Vector3.up * spawnOffsetY;
        Cup cup = Instantiate(cupPrefab, spawnPos, Quaternion.identity);

        player.PickUp(cup.gameObject);
    }

    // Doesn't need an OnHold case just yet
    public void OnHold()
    {
       
    }

    // Doesn't need an OnRelease case just yet
    public void OnRelease(Vector3 releasePos) { }
}
