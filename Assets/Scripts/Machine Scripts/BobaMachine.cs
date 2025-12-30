using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobaMachine : Machine
{
    [SerializeField] private Transform emitter;
    [SerializeField] private GameObject bobaToSpawn;

    //[SerializeField] private int bobaToEmit = 1;
    private bool canEmit = true;

    [SerializeField] private float timeBetweenEmit = .1f;

    [Header("Obj Pooling settings")]
    [SerializeField] private Transform poolParent; 
    [SerializeField] private int poolSize = 30;
    private Queue<GameObject> bobaPool = new Queue<GameObject>();

    private void Awake()
    {
        // obj pool setup
        for (int i  = 0; i < poolSize; i++)
        {
            GameObject boba = Instantiate(bobaToSpawn, poolParent);
            boba.GetComponent<Boba>().SetOwner(this);
            boba.SetActive(false);
            bobaPool.Enqueue(boba);
        }
    }

    public override void TriggerAction()
    {
        Debug.Log("Action received");
        EmitBoba();
    }

    private void EmitBoba()
    {
        if (!canEmit)
            return;

        if(bobaPool.Count > 0)
        {
            GameObject boba = bobaPool.Dequeue();
            boba.transform.position = emitter.position;
            boba.transform.rotation = Quaternion.identity;
            boba.SetActive(true);

            bobaPool.Enqueue(boba); 
        }

        StartCoroutine(BobaCooldown());
    }

    public void ReturnToPool(GameObject boba)
    {
        boba.SetActive(false);
        boba.transform.SetParent(poolParent);
        bobaPool.Enqueue(boba);
    }

    private IEnumerator BobaCooldown()
    {
        canEmit = false;
        yield return new WaitForSeconds(timeBetweenEmit);
        canEmit = true;
    }
}
