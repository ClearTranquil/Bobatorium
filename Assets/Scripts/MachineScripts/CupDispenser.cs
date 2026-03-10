using System.Collections;
using UnityEngine;

public class CupDispenser : Machine
{
    [Header("Cup Spawn Settings")]
    [SerializeField] private Cup cupPrefab;
    [SerializeField] private float timeBetweenCups = 5;
    [SerializeField] private float spawnInHandOffsetY = 1f;
    [SerializeField] private Transform conveyorCupPosition;
    [SerializeField] private GameObject conveyor;

    private Animator animator;

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
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

        StartCoroutine(CupSpawnLoop());
    }

    private void SpawnCupInHand(PlayerControls player)
    {
        Vector3 spawnPos = transform.position + Vector3.up * spawnInHandOffsetY;
        Cup cup = Instantiate(cupPrefab, spawnPos, Quaternion.identity);

        player.PickUp(cup.gameObject);
        animator.SetTrigger("removeCup");
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
