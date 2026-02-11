using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Machine : MonoBehaviour,  IInteractable
{
    public event System.Action OnMachineTriggered;

    [SerializeField] protected MachineTriggerBase trigger;
    public MachineTriggerBase GetTrigger() => trigger;
    private Coroutine employeeWorkRoutine;
    private Employee activeEmployee;

    [Header("Upgrades")]
    [SerializeField] private List<Upgrade> availableUpgrades = new();
    protected readonly List<UpgradeState> upgradeStates = new();
    Dictionary<Upgrade, UpgradeState> upgradeStateDict = new Dictionary<Upgrade, UpgradeState>();

    [Header("Snap Points")]
    protected CupSnapPoint[] cupSnapPoints;
    protected EmployeeSnapPoint[] employeeSnapPoints;

    [Header("Cup Ejection")]
    [SerializeField] private bool hasEjectUpgrade = false;
    [SerializeField] private float ejectCheckInterval = 0.1f;
    private Coroutine cupEjectRoutine;

    /*----------Upgrade Events and Init--------------*/

    protected virtual void Awake()
    {
        CacheSnapPoints();
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

    protected void CacheSnapPoints()
    {
        cupSnapPoints = GetComponentsInChildren<CupSnapPoint>(true);
        employeeSnapPoints = GetComponentsInChildren<EmployeeSnapPoint>(true);
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        CacheSnapPoints();
    }
#endif

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
        // If an upgrade can be applied to any machine, put it here
        var state = GetUpgradeState(m_upgrade);
        if(state == null)
        {
            Debug.LogWarning($"{name} cannot apply upgrade {m_upgrade.name}");
            return;
        }
        Debug.Log($"{name} applying {m_upgrade.upgradeName}, old level: {state.level - 1}, new level: {state.level}");

        if (m_upgrade.upgradeID == "AutoEject")
        {
            hasEjectUpgrade = true;

            // Start the coroutine if it hasnt already started
            if (cupEjectRoutine == null)
                cupEjectRoutine = StartCoroutine(CupEjectionLoop());
        }

        state.Apply();
        OnUpgradeApplied(m_upgrade, state);
    }

    protected virtual void OnUpgradeApplied(Upgrade m_upgrade, UpgradeState m_state)
    {
        bool handled = HandleUpgradeEvent(this, m_upgrade, m_state.level);
        UpgradeEvents.InvokeUpgradeApplied(this, m_upgrade, m_state.level);
    }

    protected UpgradeState GetUpgradeState(Upgrade m_upgrade)
    {
        if (upgradeStateDict.TryGetValue(m_upgrade, out var state))
            return state;

        return null;
    }

    /*--------------Upgrade Values---------------*/
    public float GetUpgradeValue(Upgrade m_upgrade)
    {
        var state = GetUpgradeState(m_upgrade);
        return state != null ? state.CurrentValue : 0f;
    }

    public int GetUpgradeLevel(Upgrade m_upgrade)
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

    public CupSnapPoint GetAvailableSnapPoint()
    {
        foreach (var snap in cupSnapPoints)
        {
            if (!snap.IsOccupied && !snap.IsBusy)
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

    public virtual void OnCupInserted()
    {
        // May be used later
    }

    /*----------Employee Interaction------------*/
    public void SetActiveEmployee(Employee employee)
    {
        activeEmployee = employee;
        Debug.Log(activeEmployee + " assigned to " + this);
    }

    public void RemoveActiveEmployee(Employee employee)
    {
        if (activeEmployee == employee)
        {
            StopEmployeeWork();
            activeEmployee = null;
        }
    }

    public bool HasAnyCup()
    {
        foreach(var snap in cupSnapPoints)
        {
            if (snap.IsOccupied)
                return true;
        }

        return false;
    }

    public virtual bool CheckCupCompletion()
    {
        // Each machine has different cup completion checks
        return false;
    }

    public virtual bool CanEmployeeWork()
    {
        return HasAnyCup() && !CheckCupCompletion();
    }

    public void ActivateByEmployee(float workSpeed)
    {
        StopEmployeeWork();

        employeeWorkRoutine = StartCoroutine(EmployeeWorkLoop(activeEmployee));
    }

    public virtual void StartWork(Employee employee)
    {
        if (employeeWorkRoutine != null)
            return;

        employeeWorkRoutine = StartCoroutine(EmployeeWorkLoop(employee));
    }

    protected virtual IEnumerator EmployeeWorkLoop(Employee employee)
    {
        yield break;
    }

    protected virtual float GetWorkInterval(float workSpeed)
    {
        return 0.5f / workSpeed;
    }

    public void StopEmployeeWork()
    {
        if (employeeWorkRoutine != null)
        {
            StopCoroutine(employeeWorkRoutine);
            employeeWorkRoutine = null;
        }

        // Stop the trigger if applicable
        trigger?.StopOperating();
    }

    /*------------Cup Ejection-----------*/
    protected virtual void OnEnable()
    {
        if (hasEjectUpgrade)
        {
            cupEjectRoutine = StartCoroutine(CupEjectionLoop());
        }
    }

    protected virtual void OnDisable()
    {
        if (cupEjectRoutine != null)
        {
            StopCoroutine(cupEjectRoutine);
            cupEjectRoutine = null;
        }
    }

    private IEnumerator CupEjectionLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(ejectCheckInterval);

        while (true)
        {
            foreach (var snap in cupSnapPoints)
            {
                if (snap.Occupant != null && snap.CanReleaseCup())
                {
                    // Let the machine specific completion logic decide
                    if (CheckCupCompletionForSnap(snap))
                    {
                        snap.TryEject();
                    }
                }
            }

            yield return wait;
        }
    }

    protected virtual bool CheckCupCompletionForSnap(CupSnapPoint snap)
    {
        return CheckCupCompletion();
    }
}
