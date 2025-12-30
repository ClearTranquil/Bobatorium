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
        
        // detatch from machine when cup is held
        transform.SetParent(null);
    }


    public void OnRelease(Vector3 releasePos)
    {
        //Debug.Log("Cup released");

        // Check if the cup should be snapping to a nearby snap point 
        Collider[] hits = Physics.OverlapSphere(releasePos, 0.2f);
        foreach (var hit in hits)
        {
            SnapPoints snap = hit.GetComponent<SnapPoints>();
            if (snap != null)
            {
                // cup tells the snap point that it was just placed there
                snap.TrySnapCup(this);
                break;
            }
        }

        // Logic for cups not dropped in machine here
    }

    public void OnHold()
    {
        // What the cup should do while held 
    }
}
