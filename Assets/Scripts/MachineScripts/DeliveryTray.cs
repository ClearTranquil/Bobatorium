using System.Collections;
using System;
using UnityEngine;

public class DeliveryTray : Machine
{
    [SerializeField] private float timeBetweenCups = .5f;
    [SerializeField] private GameObject slotUpgrade1;
    [SerializeField] private GameObject slotUpgrade2;

    public override void TriggerAction()
    {
        base.TriggerAction();
        DeliverAll();
    }

    protected override bool HandleUpgradeEvent(Machine m_machine, Upgrade m_upgrade, int m_newLevel)
    {
        if (!base.HandleUpgradeEvent(m_machine, m_upgrade, m_newLevel))
            return false;


        if (m_upgrade.upgradeID == "AddCupSlot")
        {
            Debug.Log($"Upgrade event received. newLevel={m_newLevel}, stackValues={string.Join(",", m_upgrade.stackValues)}");
            AddSlots(Mathf.RoundToInt(m_upgrade.stackValues[m_newLevel - 1]));
            return true;
        }

        return false;
    }

    private void AddSlots(int level)
    {
        switch (level)
        {
            case 1:
                slotUpgrade1.SetActive(true);
                cupSnapPoints[2].gameObject.SetActive(true);
                cupSnapPoints[3].gameObject.SetActive(true);
                return;
            case 2:
                slotUpgrade2.SetActive(true);
                cupSnapPoints[4].gameObject.SetActive(true);
                cupSnapPoints[5].gameObject.SetActive(true);
                return;
        }
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
            if (!snap.gameObject.activeSelf) continue;
            
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

    // Delivery tray accepts all cups, finished or not. 
    public override bool CheckSpecificCupCompletion(Cup cup)
    {
        return false;
    }

    private void ScoreCup(Cup m_cup)
    {
        SaleEvents.OnCupSold?.Invoke(m_cup);
    }
}
