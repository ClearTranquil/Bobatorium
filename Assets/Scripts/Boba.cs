using UnityEngine;

public class Boba : MonoBehaviour
{
    private BobaMachine owner;

    public void SetOwner(BobaMachine machine)
    {
        owner = machine;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Cup cup = collision.gameObject.GetComponent<Cup>();
        if (cup)
        {
            // Add boba to cup
            Debug.Log("Boba in cup :)");
        }

        Despawn();
    }

    private void Despawn()
    {
        if (owner != null)
        {
            owner.ReturnToPool(gameObject);
        }
    }
}
