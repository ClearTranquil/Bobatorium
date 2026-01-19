using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;

    // Each machine instance has its own list of UpgadeStates
    private Dictionary<Machine, Dictionary<Upgrade, UpgradeState>> machineUpgrades = new Dictionary<Machine, Dictionary<Upgrade, UpgradeState>>();

    // Registers new machines when they're purchased
    public void RegisterMachine(Machine m_machine)
    {
        if (!machineUpgrades.ContainsKey(m_machine))
        {
            machineUpgrades[m_machine] = new Dictionary<Upgrade, UpgradeState>();
        }
    }

    // Applies an upgrade to a machine
    public void ApplyUpgrade(Machine m_machine, Upgrade m_upgrade)
    {
        if (!machineUpgrades.TryGetValue(m_machine, out var upgrades))
        {
            Debug.LogWarning("Machine not registered with UpgradeManager");
            return;
        }

        if (!upgrades.TryGetValue(m_upgrade, out var state))
        {
            state = new UpgradeState(m_upgrade);
            upgrades.Add(m_upgrade, state);
        }

        if (state.IsMaxed)
        {
            Debug.Log("Upgrades maxed out for this machine");
            return;
        }

        state.Apply();
    }

    public float GetTotalUpgradeValue(Machine m_machine, System.Func<UpgradeState, float> valueSelector)
    {
        if (!machineUpgrades.ContainsKey(m_machine)) return 0f;

        float total = 0f;
        foreach (var state in machineUpgrades[m_machine].Values) // <-- use .Values
        {
            total += valueSelector(state);
        }
        return total;
    }

    public List<UpgradeState> GetUpgrades(Machine m_machine)
    {
        if (!machineUpgrades.ContainsKey(m_machine)) return new List<UpgradeState>();
        return new List<UpgradeState>(machineUpgrades[m_machine].Values);
    }
}
