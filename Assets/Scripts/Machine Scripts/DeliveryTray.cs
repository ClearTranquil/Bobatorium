using UnityEngine;

public class DeliveryTray : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Cup cup = other.GetComponent<Cup>();

        if (cup == null)
            return;

        //Deliver(cup);
    }

    //private void Deliver(Cup m_cup)
    //{
    //    Debug.Log("Tea delivered!");
    //    Destroy(m_cup.transform.root.gameObject);
    //}
}
