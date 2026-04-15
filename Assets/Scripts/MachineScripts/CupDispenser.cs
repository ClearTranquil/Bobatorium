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
    [SerializeField] private GameObject autoDispenseLight;

    [SerializeField] private float maxCupsPerSecond = 2f;
    private float spawnTimer = 0f;

    private Animator animator;

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
    }

    // Cup dispense interval is tied to the speed of the conveyor belt
    private void Update()
    {
        if (conveyor == null)
            return;

        Conveyor c = conveyor.GetComponentInChildren<Conveyor>();
        float speed = c.NormalizedSpeed;

        if (speed <= 0.01f)
        {
            spawnTimer = 0f;
            autoDispenseLight.SetActive(false);
            return;
        } else
        {
            autoDispenseLight.SetActive(true);
        }

        float cupsPerSecond = (speed * maxCupsPerSecond) / 3.5f;

        spawnTimer += Time.deltaTime * cupsPerSecond;

        while (spawnTimer >= 1f)
        {
            SpawnCupOnBelt();
            spawnTimer -= 1f;
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

        return false;
    }

    private void SpawnConveyor()
    {
        conveyor.SetActive(true);
    }

    private void SpawnCupOnBelt()
    {
        Vector3 spawnPos = conveyorCupPosition.position;
        Cup cup = Instantiate(cupPrefab, spawnPos, Quaternion.identity);
        cup.TogglePhysics(true);
        cup.gameObject.transform.rotation = Quaternion.identity;
        animator.SetTrigger("removeCup");
    }
}
