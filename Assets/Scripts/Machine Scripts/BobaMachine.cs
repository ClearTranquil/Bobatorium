using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobaMachine : Machine
{
    [SerializeField] private Transform emitter;

    private bool canEmit = true;
    [SerializeField] private float timeBetweenEmit = .1f;
    [SerializeField] private float timeBetweenTrigger = .6f;

    [Header("Obj Pooling settings")]
    [SerializeField] private Transform poolParent; 
    [SerializeField] private int poolSize = 30;
    private Queue<GameObject> bobaPool = new Queue<GameObject>();

    [Header("Upgrade Variables")]
    private int bobaToEmit = 1;
    [SerializeField] private GameObject bobaToSpawn;

    protected override void Awake()
    {
        // sets up upgradeStates in base Machine
        base.Awake();

        // obj pool setup
        for (int i = 0; i < poolSize; i++)
        {
            GameObject boba = Instantiate(bobaToSpawn, poolParent);
            boba.GetComponent<Boba>().SetOwner(this);
            boba.SetActive(false);
            bobaPool.Enqueue(boba);
        }
    }

    public override void TriggerAction()
    {
        //Debug.Log("Action received");
        if (canEmit)
        {
            canEmit = false;
            StartCoroutine(EmitBoba());
        }
    }

    private IEnumerator EmitBoba()
    {
        for (int i = 0; i < bobaToEmit; i++)
        {
            if (bobaPool.Count == 0) break;

            GameObject boba = bobaPool.Dequeue();
            boba.transform.position = emitter.position;
            boba.transform.rotation = Quaternion.identity;
            boba.SetActive(true);
            bobaPool.Enqueue(boba);

            yield return new WaitForSeconds(timeBetweenEmit);
        }

        yield return new WaitForSeconds(timeBetweenTrigger);
        canEmit = true;
    }

    public void ReturnToPool(GameObject boba)
    {
        boba.SetActive(false);
        boba.transform.SetParent(poolParent);
        bobaPool.Enqueue(boba);
    }

    /*-----------------Upgrade Interaction-------------*/

    protected override bool HandleUpgradeEvent(Machine m_machine, Upgrade m_upgrade, int m_newLevel)
    {
        if (!base.HandleUpgradeEvent(m_machine, m_upgrade, m_newLevel))
            return false;

        if (m_upgrade.upgradeID == "BobaPerClick")
        {
            Debug.Log($"Upgrade event received. newLevel={m_newLevel}, stackValues={string.Join(",", m_upgrade.stackValues)}");
            bobaToEmit = Mathf.RoundToInt(m_upgrade.stackValues[m_newLevel - 1]);
            Debug.Log($"{name} bobaToEmit updated to {bobaToEmit}");
            return true;
        }

        return false;
    }
}
