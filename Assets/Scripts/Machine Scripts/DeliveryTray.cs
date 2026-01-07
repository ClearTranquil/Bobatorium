using UnityEngine;

public class DeliveryTray : Machine
{
    public override void TriggerAction()
    {
        //Debug.Log("Action received");
        DeliverAll();
    }

    /* Called when player presses the delivery tray button.
     * Iterates through all "slots" on the tray to check if a cup is there. 
     * If a cup is there, check if it meets the requirements to be sold. */
    public void DeliverAll()
    {
        foreach(var snap in snapPoints)
        {
            if(snap.OccupiedCup != null)
            {
                Cup cup = snap.OccupiedCup;
                bool cupValid = false;

                // Insert cup validation logic here!! 
                if(cup.isBobaFull() && cup.isTeaFull() && cup.GetIsSealed())
                {
                    cupValid = true;
                }

                if (cupValid)
                {
                    Debug.Log("Sold!");
                    Destroy(cup.gameObject);
                    snap.Clear();
                } else
                {
                    Debug.Log("Cup rejected");
                    //Destroy(cup.gameObject);
                }

            } else
            {
                //Debug.Log("No cup to sell!");
            }
        }
    }
}
