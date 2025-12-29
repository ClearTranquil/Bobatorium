using UnityEngine;

public class DeliveryTray : MonoBehaviour
{
    public void Deliver(GameObject m_cup)
    {
        // Check if cup meets delivery requirements here!
        
        // Successful cup delivery
        if(m_cup != null && m_cup.GetComponent<Cup>() != null)
        {
            Debug.Log("Tea delivered!");
            Destroy(m_cup);
        }

        // Unsuccessful cup delivery logic here
        Debug.Log("Delivery unsuccessful");
    }
}
