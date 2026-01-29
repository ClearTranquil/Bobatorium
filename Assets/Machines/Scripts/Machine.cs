using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public abstract class Machine : MonoBehaviour,  IInteractable
{
    public event System.Action OnMachineTriggered;

    [Header("Upgrades")]
    [SerializeField] private List<Upgrade> availableUpgrades = new();
    protected readonly List<UpgradeState> upgradeStates = new();
    Dictionary<Upgrade, UpgradeState> upgradeStateDict = new Dictionary<Upgrade, UpgradeState>();

    [Header("Snap Points")]
    [SerializeField] protected SnapPoints[] snapPoints;

    /*----------Upgrade Events and Init--------------*/

    protected virtual void Awake()
    {
        InitializeUpgrades();
    }

    private void InitializeUpgrades()
    {
        upgradeStates.Clear();
        upgradeStateDict = upgradeStateDict ?? new Dictionary<Upgrade, UpgradeState>();
        upgradeStateDict.Clear();

        if (availableUpgrades == null) return;

        foreach (var upgrade in availableUpgrades)
        {
            if (upgrade == null || upgradeStateDict.ContainsKey(upgrade)) continue;

            UpgradeState state = new UpgradeState(upgrade);
            upgradeStates.Add(state);
            upgradeStateDict.Add(upgrade, state);
        }
    }

    protected virtual bool HandleUpgradeEvent(Machine machine, Upgrade upgrade, int newLevel)
    {
        // Only listen to upgrade event if it was meant for this machine. Ignore anything else. 
        if (machine != this) return false;
        if (newLevel <= 0) return false;

        return true;
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
        Debug.Log($"{name} applying {m_upgrade.upgradeName}, old level: {state.level - 1}, new level: {state.level}");

        state.Apply();
        OnUpgradeApplied(m_upgrade, state);
    }

    protected virtual void OnUpgradeApplied(Upgrade m_upgrade, UpgradeState m_state)
    {
        // 1. Let THIS machine apply the upgrade immediately
        bool handled = HandleUpgradeEvent(this, m_upgrade, m_state.level);

        if (!handled)
        {
            Debug.LogWarning(
                $"{name} received upgrade '{m_upgrade.upgradeID}' but did not handle it.\n" +
                $"Check upgradeID spelling or missing implementation."
            );
        }

        // 2. Broadcast AFTER it was applied
        UpgradeEvents.InvokeUpgradeApplied(this, m_upgrade, m_state.level);
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
        OnMachineTriggered?.Invoke();
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

    /*------------Interactions---------------*/

    public bool CanInteract(PlayerControls player)
    {
        return true;
    }

    public void Interact(PlayerControls player)
    {
        
    }

    public void OnRightClick(PlayerControls player)
    {
        UpgradeUIManager.Instance.Open(this);
    }

    public void OnRelease(Vector3 releasePos)
    {
        
    }

    public void OnHold()
    {
        
    }
}
