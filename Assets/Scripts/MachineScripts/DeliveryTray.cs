using System.Collections;
using System;
using UnityEngine;

public class DeliveryTray : Machine
{
    [SerializeField] private float timeBetweenCups = .5f;

    public override void TriggerAction()
    {
        base.TriggerAction();
        DeliverAll();
    }

    /* Called when player presses the delivery tray button.
     * Iterates through all "slots" on the tray to check if a cup is there. 
     * If a cup is there, check if it meets the requirements to be sold. */
    public void DeliverAll()
    {
        StartCoroutine(ScanCups());
    }

    private IEnumerator ScanCups()
    {
        foreach (var snap in cupSnapPoints)
        {
            if (snap.Occupant != null)
            {
                Cup cup = snap.Occupant;
                bool cupValid = false;

                // Insert cup validation logic here!! 
                if (cup.isBobaFull() && cup.isTeaFull() && cup.GetIsSealed())
                {
                    cupValid = true;
                }

                if (cupValid)
                {
                    Debug.Log("Sold!");
                    ScoreCup(cup);
                    //Destroy(cup.gameObject); Cups are now destroyed by NPCs. Kinda like they're drinking it. That makes sense right? 
                    snap.Clear();
                }
                else
                {
                    Debug.Log("Cup rejected");
                    //Destroy(cup.gameObject);
                }
            }

            yield return new WaitForSeconds(timeBetweenCups);
        }
    }

    private void ScoreCup(Cup m_cup)
    {
        SaleEvents.OnCupSold?.Invoke(m_cup);
    }
}
