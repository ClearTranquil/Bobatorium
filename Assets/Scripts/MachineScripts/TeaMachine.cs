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
        MachineLever lever = trigger as MachineLever;
        if (lever == null || !HasAnyIncompleteCup())
            yield break;

        // Exaggerate the difference in workspeed for this specific machine
        float effectiveSpeed = Mathf.Clamp(employee.GetEffectiveWorkSpeed(), 0.25f, 1f);
        lever.RemoteActivate(effectiveSpeed / 0.5f);

        while (!CheckCupCompletion())
            yield return null;

        employee.OnCupCompleted();
        lever.StopOperating();
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
