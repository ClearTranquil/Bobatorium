using UnityEngine;

public class TeaMachine : Machine
{
    [Header("Pour Settings")]
    [SerializeField] private float pourRate = .25f;
    [SerializeField] private Transform spout;

    private bool isPouring;


    public override void TriggerAction()
    {
        isPouring = true;
    }

    public override void StopTrigger()
    {
        isPouring = false;
    }

    public bool IsPouring => isPouring;
    public float PourRate => pourRate;

    public void SetPourRate(float m_pourRate)
    {
        pourRate = m_pourRate;
    }
}
