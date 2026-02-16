using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TeaMachine : Machine
{
    [Header("Pour Settings")]
    [SerializeField] private float basePourRate = 0.25f;
    private float pourRate = .25f;
    [SerializeField] private Transform[] spouts;

    private bool isPouring;
    [SerializeField] private GameObject slotUpgrade1;
    [SerializeField] private GameObject slotUpgrade2;

    protected override void Awake()
    {
        base.Awake();

        pourRate = basePourRate;
    }


    public override void TriggerAction()
    {
        base.TriggerAction();
        isPouring = true;
    }

    public override void StopTrigger()
    {
        isPouring = false;
    }

    public override bool CheckCupCompletion()
    {
        foreach (var snap in cupSnapPoints)
        {
            ICupInfo cup = snap;
            if (cup != null)
            {             
                if (cup.TeaFull)
                    return true;
            }
        }

        return false;
    }

    public override bool CheckSpecificCupCompletion(Cup cup)
    {
        return cup.TeaFill;
    }

    public bool IsPouring => isPouring;
    public float PourRate => pourRate;

    public void SetPourRate(float m_pourRate)
    {
        pourRate = m_pourRate;
    }

    /*-----------------Upgrade Interaction-------------*/

    protected override bool HandleUpgradeEvent(Machine m_machine, Upgrade m_upgrade, int m_newLevel)
    {
        if (!base.HandleUpgradeEvent(m_machine, m_upgrade, m_newLevel))
            return false;

        if (m_upgrade.upgradeID == "TeaPourSpeed")
        {
            Debug.Log($"Upgrade event received. newLevel={m_newLevel}, stackValues={string.Join(",", m_upgrade.stackValues)}");
            pourRate = m_upgrade.stackValues[m_newLevel - 1];
            return true;
        }

        if (m_upgrade.upgradeID == "AddCupSlot")
        {
            Debug.Log($"Upgrade event received. newLevel={m_newLevel}, stackValues={string.Join(",", m_upgrade.stackValues)}");
            ActivateSpout(Mathf.RoundToInt(m_upgrade.stackValues[m_newLevel - 1]));
            return true;
        }

        return false;
    }

    private void ActivateSpout(int upgradeLevel)
    {
        switch (upgradeLevel)
        {
            case 1:
                slotUpgrade1.SetActive(true);
                spouts[1].gameObject.SetActive(true);
                cupSnapPoints[1].gameObject.SetActive(true);
                Debug.Log("Activating cup slot 1");
                return;
            case 2:
                slotUpgrade2.SetActive(true);
                spouts[2].gameObject.SetActive(true);
                cupSnapPoints[2].gameObject.SetActive(true);
                Debug.Log("Activating cup slot 2");
                return;
        }
    }

    protected override IEnumerator EmployeeWorkLoop(Employee employee)
    {
        
        MachineLever lever = trigger as MachineLever;
        if (lever == null || !HasAnyIncompleteCup() || employee == null)
            yield break;

        // Exaggerate the difference in workspeed for this specific machine
        float effectiveSpeed = Mathf.Clamp(employee.GetEffectiveWorkSpeed(), 0.25f, 1f);
        lever.RemoteActivate(effectiveSpeed / 0.5f);

        while (!CheckCupCompletion())
            yield return null;

        employee.OnCupCompleted();
        lever.StopOperating();
        StopEmployeeWork();
    }

    private bool HasAnyIncompleteCup()
    {
        foreach (var snap in cupSnapPoints)
        {
            ICupInfo cup = snap;
            if (cup != null && !cup.TeaFull)
                return true;
        }
        return false;
    }
}
