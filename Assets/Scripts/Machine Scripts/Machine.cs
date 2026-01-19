using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public abstract class Machine : MonoBehaviour
{
    [Header("Upgrades")]
    [SerializeField] private List<Upgrade> availableUpgrades = new();
    protected readonly List<UpgradeState> upgradeStates = new();
    Dictionary<Upgrade, UpgradeState> upgradeStateDict;

    [Header("Snap Points")]
    [SerializeField] protected SnapPoints[] snapPoints;

    protected virtual void Awake()
    {
        InitializeUpgrades();
    }

    private void InitializeUpgrades()
    {
        upgradeStates.Clear();
        upgradeStateDict.Clear();

        foreach (var upgrade in availableUpgrades)
        {
            UpgradeState state = new UpgradeState(upgrade);
            upgradeStates.Add(state);
            upgradeStateDict.Add(upgrade, state);
        }
    }

    /*--------------Upgrade Queries---------------*/
    public IReadOnlyList<UpgradeState> GetUpgradeStates()
    {
        return upgradeStates;
    }

    public bool CanApplyUpgrade(Upgrade m_upgrade)
    {
        var state = GetUpgradeState(m_upgrade);
        return state != null && !state.IsMaxed;
    }

    public void ApplyUpgrade(Upgrade m_upgrade)
    {
        var state = GetUpgradeState(m_upgrade);
        if(state == null)
        {
            Debug.LogWarning($"{name} cannot apply upgrade {m_upgrade.name}");
            return;
        }

        state.Apply();
        OnUpgradeApplied(m_upgrade, state);
    }

    protected virtual void OnUpgradeApplied(Upgrade m_upgrade, UpgradeState m_state)
    {
        // Optional, used by some machines
    }

    protected UpgradeState GetUpgradeState(Upgrade m_upgrade)
    {
        if (upgradeStateDict.TryGetValue(m_upgrade, out var state))
            return state;

        return null;
    }

    /*--------------Upgrade Values---------------*/
    protected float GetUpgradeValue(Upgrade m_upgrade)
    {
        var state = GetUpgradeState(m_upgrade);
        return state != null ? state.CurrentValue : 0f;
    }

    protected int GetUpgradeLevel(Upgrade m_upgrade)
    {
        var state = GetUpgradeState(m_upgrade);
        return state != null ? state.level : 0;
    }

    /*----------------Triggers--------------*/

    public virtual void TriggerAction()
    {
        // The machine's payload when triggered
    }

    public virtual void StopTrigger()
    {
        // Stops the payload, if necessary
    }

    public SnapPoints GetAvailableSnapPoint()
    {
        foreach(var snap in snapPoints)
        {
            if (!snap.IsOccupied)
                return snap;
        }

        return null;
    }
}
