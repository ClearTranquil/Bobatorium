using UnityEngine;

public class Cup : MonoBehaviour, IInteractable
{
    private int bobaCount = 0;
    private float teaFill;
    private bool isSealed;

    private int GetBobaCount()
        { return bobaCount; }

    private float GetTeaFill()
        { return teaFill; }

    private bool GetIsSealed()
        { return isSealed; }

    public void Interact(PlayerControls player)
    {
        player.PickUp(gameObject);
    }

    public void OnRelease()
    {
        Debug.Log("Cup released");
        
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.8f);
        
        foreach (var hit in hits)
        {
            // Check if its being placed on the delivery tray
            DeliveryTray tray = hit.GetComponent<DeliveryTray>();
            if (tray != null)
            {
                Debug.Log("Tray sensed");
                tray.Deliver(gameObject);
                return;
            }

            // Check if cup is on conveyor belt here
        }

        // Behavior for dropping the cup elsewhere goes here 
    }
}
