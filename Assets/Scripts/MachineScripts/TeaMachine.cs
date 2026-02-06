using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TeaMachine : Machine
{
    [Header("Pour Settings")]
    [SerializeField] private float basePourRate = 0.25f;
    private float pourRate = .25f;
    [SerializeField] private Transform spout;

    private bool isPouring;

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

        return false;
    }

    protected override IEnumerator EmployeeWorkLoop(Employee employee)
    {
        // Get the lever trigger
        MachineLever lever = trigger as MachineLever;
        if (lever == null)
            yield break;

        // Start moving the lever using employee's effective work speed
        lever.RemoteActivate(employee.GetEffectiveWorkSpeed());

        while (!CheckCupCompletion())
        {
            yield return null;
        }

        lever.StopOperating();
    }
}
