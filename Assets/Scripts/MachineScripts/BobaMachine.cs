using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobaMachine : Machine
{
    [SerializeField] private Transform[] emitters;
    [SerializeField] private GameObject slotUpgrade1;
    [SerializeField] private GameObject slotUpgrade2;

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
        base.TriggerAction();

        if (canEmit)
        {
            canEmit = false;
            StartCoroutine(EmitBoba());
        }
    }

    public override bool CheckCupCompletion()
    {
        foreach(var snap in cupSnapPoints)
        {
            ICupInfo cup = snap;
            if(cup != null)
            {
                if (cup.BobaFull)
                    return true;
            }
        }

        return false;
    }

    public override bool CheckSpecificCupCompletion(Cup cup)
    {
        return cup.BobaFull;
    }

    private IEnumerator EmitBoba()
    {
        for (int i = 0; i < bobaToEmit; i++)
        {
            foreach (Transform emitter in emitters)
            {
                if (!emitter.gameObject.activeSelf)
                    continue;

                if (bobaPool.Count == 0)
                    yield break;

                GameObject boba = bobaPool.Dequeue();

                boba.transform.position = emitter.position;
                boba.transform.rotation = Quaternion.identity;

                Rigidbody rb = boba.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.angularVelocity = Vector3.zero;
                    rb.linearVelocity = Vector3.zero;
                }

                boba.SetActive(true);
            }

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

        if (m_upgrade.upgradeID == "AddCupSlot")
        {
            Debug.Log($"Upgrade event received. newLevel={m_newLevel}, stackValues={string.Join(",", m_upgrade.stackValues)}");
            ActivateEmitter(Mathf.RoundToInt(m_upgrade.stackValues[m_newLevel - 1]));
            Debug.Log($"{name} bobaToEmit updated to {bobaToEmit}");
            return true;
        }

        return false;
    }

    private void ActivateEmitter(int upgradeLevel)
    {
        switch (upgradeLevel)
        {
            case 1: 
                slotUpgrade1.SetActive(true);
                emitters[1].gameObject.SetActive(true);
                cupSnapPoints[1].gameObject.SetActive(true);
                Debug.Log("Activating cup slot 1");
                return;
            case 2: 
                slotUpgrade2.SetActive(true);
                emitters[2].gameObject.SetActive(true);
                cupSnapPoints[2].gameObject.SetActive(true);
                Debug.Log("Activating cup slot 2");
                return;
        }
    }

    /*----------Employee Interaction---------*/
    protected override IEnumerator EmployeeWorkLoop(Employee employee)
    {
        if (employee == null)
            yield break;

        while (HasAnyCup() && !CheckCupCompletion() && employee.CurrentMachine == this)
        {
            TriggerAction();

            float waitTime = timeBetweenTrigger * (1f / employee.GetEffectiveWorkSpeed()) * 6f;
            float elapsed = 0f;

            while (elapsed < waitTime)
            {
                if (employee.CurrentMachine != this)
                    yield break;

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        employee.OnCupCompleted();
        StopEmployeeWork();
        yield break;
    }
}
