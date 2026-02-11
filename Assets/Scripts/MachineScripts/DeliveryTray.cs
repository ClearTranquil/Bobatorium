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
            // Grab the cup info interface
            ICupInfo cupInfo = snap;
            
            if (cupInfo != null)
            {
                Cup cup = snap.GetComponentInChildren<Cup>();

                if (IsCupComplete(cupInfo))
                {
                    Debug.Log("Sold!");
                    cup.OnCupValiated();

                    yield return new WaitForSeconds(.5f);

                    ScoreCup(cup.GetComponent<Cup>());
                    //Destroy(cup.gameObject); Cups are now destroyed by NPCs. Kinda like they're drinking it. That makes sense right? 
                    snap.Clear();
                } else
                {
                    snap.TryEject();
                }
            }

            yield return new WaitForSeconds(timeBetweenCups);
        }
    }

    private bool IsCupComplete(ICupInfo cupInfo)
    {
        return cupInfo.BobaFull && cupInfo.TeaFull && cupInfo.IsSealed;
    }

    private void ScoreCup(Cup m_cup)
    {
        SaleEvents.OnCupSold?.Invoke(m_cup);
    }
}
