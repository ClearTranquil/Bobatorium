using UnityEngine;

public class TeaMachine : Machine
{
    [SerializeField] private float amountToDispense = 1f;
    
    public override void TriggerAction()
    {
        PourTea();
    }

    private void PourTea()
    {
        Debug.Log("Tea unit poured" + amountToDispense);
    }
}
