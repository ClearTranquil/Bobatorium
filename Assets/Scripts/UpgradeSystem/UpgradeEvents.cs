using System;
using Unity.VisualScripting;
using UnityEngine;

public class UpgradeEvents : MonoBehaviour
{
    // This event is fired whenever an upgrade is purchased
    // It delivers the machine instance, upgrade being used, and what upgrade level to use. The machines figure out the rest.
    public static event Action<Machine, Upgrade, int> OnUpgradeApplied;

    public static void InvokeUpgradeApplied(Machine m_machine, Upgrade m_upgrade, int newLevel)
    {
        OnUpgradeApplied?.Invoke(m_machine, m_upgrade, newLevel);
    }
}
