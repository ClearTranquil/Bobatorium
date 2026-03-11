using System.Collections;
using UnityEngine;

public class CupDispenser : Machine
{
    [Header("Cup Spawn Settings")]
    [SerializeField] private Cup cupPrefab;
    [SerializeField] private float spawnInHandOffsetY = 1f;
    [SerializeField] private Transform conveyorCupPosition;
    [SerializeField] private GameObject conveyor;

    [Header("Auto Cup Dispense")]
    private bool dispenseTriggerActive = false;
    private bool autoDispenseActive = false;
    [SerializeField] private float timeBetweenCups = 5;
    [SerializeField] private GameObject autoDispenseLight;
    private Coroutine cupDispense;

    private Animator animator;

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();

        if (dispenseTriggerActive)
        {
            trigger.gameObject.SetActive(true);
            autoDispenseLight.SetActive(true);
        } else
        {
            trigger.gameObject.SetActive(false);
            autoDispenseLight.SetActive(false);
        }
    }

    public override bool CanInteract(PlayerControls player)
    {
        return true;
    }

    // Spawns a cup at the player's hand 
    public override void Interact(PlayerControls player)
    {
        SpawnCupInHand(player);
    }

    private void SpawnCupInHand(PlayerControls player)
    {
        Vector3 spawnPos = transform.position + Vector3.up * spawnInHandOffsetY;
        Cup cup = Instantiate(cupPrefab, spawnPos, Quaternion.identity);

        player.PickUp(cup.gameObject);
        animator.SetTrigger("removeCup");
    }

    /*================Upgrades=================*/

    protected override bool HandleUpgradeEvent(Machine m_machine, Upgrade m_upgrade, int m_newLevel)
    {
        if (!base.HandleUpgradeEvent(m_machine, m_upgrade, m_newLevel))
            return false;

        if (m_upgrade.upgradeID == "SpawnConveyor")
        {
            Debug.Log($"Upgrade event received. newLevel={m_newLevel}, stackValues={string.Join(",", m_upgrade.stackValues)}");
            SpawnConveyor();
            return true;
        }

        if(m_upgrade.upgradeID == "AutoCupDispense")
        {
            Debug.Log($"Upgrade event received. newLevel={m_newLevel}, stackValues={string.Join(",", m_upgrade.stackValues)}");
            SpawnTrigger();
            return true;
        }

        return false;
    }

    private void SpawnConveyor()
    {
        conveyor.SetActive(true);
    }

    private void SpawnTrigger()
    {
        trigger.gameObject.SetActive(true);
    }

    // Triggering the machine just turns auto cup dispensing on/off. This button is hidden by default, you have to purchase the upgrade to see it. 
    public override void TriggerAction()
    {
        base.TriggerAction();

        ToggleAutoCupDispense();
    }

    private void ToggleAutoCupDispense()
    {
        autoDispenseActive = !autoDispenseActive;
        
        if (autoDispenseActive)
        {
            autoDispenseLight.SetActive(true);
            cupDispense = StartCoroutine(CupSpawnLoop());
        } else
        {
            autoDispenseLight.SetActive(false);
            StopCoroutine(cupDispense);
        }
    }

    private void SpawnCupOnBelt()
    {
        Vector3 spawnPos = conveyorCupPosition.position;
        Cup cup = Instantiate(cupPrefab, spawnPos, Quaternion.identity);
        cup.TogglePhysics(true);
        cup.gameObject.transform.rotation = Quaternion.identity;
        animator.SetTrigger("removeCup");
    }

    private IEnumerator CupSpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenCups);
            SpawnCupOnBelt();
        }
    }
}
