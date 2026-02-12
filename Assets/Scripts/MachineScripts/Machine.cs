using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Machine : MonoBehaviour,  IInteractable
{
    [Header("Upgrades")]
    [SerializeField] private List<Upgrade> availableUpgrades = new();
    protected readonly List<UpgradeState> upgradeStates = new();
    Dictionary<Upgrade, UpgradeState> upgradeStateDict = new Dictionary<Upgrade, UpgradeState>();

    [Header("Triggers")]
    [SerializeField] protected MachineTriggerBase trigger;
    public MachineTriggerBase GetTrigger() => trigger;

    [Header("Snap Points")]
    protected CupSnapPoint[] cupSnapPoints;
    protected EmployeeSnapPoint[] employeeSnapPoints;

    [Header("Cup Ejection")]
    [SerializeField] private bool hasEjectUpgrade = false;
    [SerializeField] private float ejectCheckInterval = 0.1f;
    private Coroutine cupEjectRoutine;

    [Header("Employees")]
    private Coroutine employeeWorkRoutine;
    private Employee activeEmployee;
    
    // Events
    public event System.Action OnMachineTriggered;


    /*----------Upgrade Events and Init--------------*/

    protected virtual void Awake()
    {
        CacheSnapPoints();
        InitializeUpgrades();
    }

    private void InitializeUpgrades()
    {
        // This block of code will likely be adjusted when I add saving/loading
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

    // Set up employee and cup snap points
    protected void CacheSnapPoints()
    {
        cupSnapPoints = GetComponentsInChildren<CupSnapPoint>(true);
        employeeSnapPoints = GetComponentsInChildren<EmployeeSnapPoint>(true);
    }

    // Lets me change serialized fields in editor for testing
#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        CacheSnapPoints();
    }
#endif

    /*--------------Upgrade Queries---------------*/
    // Tells upgrade UI what level current upgrades are
    public IReadOnlyList<UpgradeState> GetUpgradeStates()
    {
        return upgradeStates;
    }

    // This is where upgrades handle their payload. Individual machines override this for their own needs. 
    // If an upgrade can be applied to any machine, put it here
    public void ApplyUpgrade(Upgrade m_upgrade)
    {
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

    // Invokes the upgrade event
    protected virtual void OnUpgradeApplied(Upgrade m_upgrade, UpgradeState m_state)
    {
        bool handled = HandleUpgradeEvent(this, m_upgrade, m_state.level);
        UpgradeEvents.InvokeUpgradeApplied(this, m_upgrade, m_state.level);
    }

    // Only listen to upgrade event if it was meant for this machine. Ignore anything else. 
    protected virtual bool HandleUpgradeEvent(Machine machine, Upgrade upgrade, int newLevel)
    {
        if (machine != this) return false;
        if (newLevel <= 0) return false;

        return true;
    }

    // Checks the current level of the upgrade
    protected UpgradeState GetUpgradeState(Upgrade m_upgrade)
    {
        if (upgradeStateDict.TryGetValue(m_upgrade, out var state))
            return state;

        return null;
    }

    /*----------------Triggers--------------*/

    // What the machine does when activated by button, lever, or ripcord. Gets overridden by specific machines.
    public virtual void TriggerAction()
    {
        OnMachineTriggered?.Invoke();
        // The machine's payload when triggered
    }

    // Stops the payload, if necessary. Gets overriden by specific machines.
    public virtual void StopTrigger()
    {
        
    }

    /*------------Interactions---------------*/

    // These are required for the playercontroller to interact with machines
    public bool CanInteract(PlayerControls player)
    {
        return true;
    }

    public void Interact(PlayerControls player)
    {
        
    }

    // Right clicking on machines opens the upgrade options
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
        // TODO: add animations and sfx when a cup is inserted
    }

    // Checks for any open snapPoints on this machine
    public CupSnapPoint GetAvailableSnapPoint()
    {
        foreach (var snap in cupSnapPoints)
        {
            if (!snap.IsOccupied && !snap.IsBusy)
                return snap;
        }
        return null;
    }

    /*----------Employee Interaction------------*/

    // Assigns an employee to this machine
    public void SetActiveEmployee(Employee employee)
    {
        activeEmployee = employee;
        Debug.Log(activeEmployee + " assigned to " + this);
    }

    // Removes employee when they are picked up
    public void RemoveActiveEmployee(Employee employee)
    {
        if (activeEmployee == employee)
        {
            StopEmployeeWork();
            activeEmployee = null;
        }
    }

    // Stop the employee's work loop if the work is finished or if the employee/cup have been removed
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

    // Scans all cupSnapPoints to see if they're occupied
    public bool HasAnyCup()
    {
        foreach(var snap in cupSnapPoints)
        {
            if (snap.IsOccupied)
                return true;
        }

        return false;
    }

    // Each machine has different cup completion checks. Ex: BobaMachine checks if BobaFull == true
    public virtual bool CheckCupCompletion()
    {
        return false;
    }

    // Tells employees if there's work to be done on this machine
    public virtual bool CanEmployeeWork()
    {
        return HasAnyCup() && !CheckCupCompletion();
    }

    // Reset's employee's current work loop, then kicks off a new work loop
    public void ActivateByEmployee(float workSpeed)
    {
        if (employeeWorkRoutine != null)
            return;

        employeeWorkRoutine = StartCoroutine(EmployeeWorkLoop(activeEmployee));
    }

    // Each machine has different means of operation, so this is overridden by other machine scripts
    protected virtual IEnumerator EmployeeWorkLoop(Employee employee)
    {
        yield break;
    }

    /*------------Cup Ejection-----------*/
    // When the upgrade is active, kick off a loop that checks if the cup is ready to be ejected
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

    // Once initiated, goes through all cupSnaps, checks if they have a cup, check if the cup meets that machine's definition of complete, then ejects. 
    private IEnumerator CupEjectionLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(ejectCheckInterval);

        while (true)
        {
            foreach (var snap in cupSnapPoints)
            {
                if (snap.Occupant != null && snap.CanReleaseCup())
                {
                    if (CheckCupCompletion())
                    {
                        snap.TryEject();
                    }
                }
            }

            yield return wait;
        }
    }
}
