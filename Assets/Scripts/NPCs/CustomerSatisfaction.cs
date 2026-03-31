using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class CustomerSatisfaction : MonoBehaviour
{
    [Header("Core Values")]
    [Range(0f, 100f)]
    [SerializeField] private float currentSatisfaction = 50f;
    [SerializeField] private float satisfactionSoftCap = 65f;
    [SerializeField] private float maxSatisfaction = 65f;

    [Header("Gain Chance")]
    [SerializeField] private float gainChance = 0.35f;
    [SerializeField] private Vector2 gainAmountRange = new Vector2(2f, 5f);
    private int failsSinceLastGain = 0;

    [Header("Penalty")]
    [SerializeField] private float failPenalty = 5f;

    public float Current => currentSatisfaction;
    public float CurrentNormalized => currentSatisfaction / 100f;
    public static Action<float, float> OnSatisfactionChanged;


    void Start()
    {
        ClampSatisfaction();
        NotifyUI();
    }

    private void OnEnable()
    {
        SaleEvents.OnCupSold += HandleSale;
        SaleEvents.OnCustomerTimedOut += HandleTimeOut;
    }

    private void OnDisable()
    {
        SaleEvents.OnCupSold -= HandleSale;
        SaleEvents.OnCustomerTimedOut -= HandleTimeOut;
    }

    private void HandleSale(Cup cup, Customer customer)
    {
        if (customer == null) return;

        if (customer.WasServedInTime)
        {
            OnSuccessfulOrder();
        }
    }

    private void HandleTimeOut(Customer customer)
    {
        if (customer == null) return;

        OnFailedOrder();
    }

    //============== Success/Failure Logic ==================
    public void OnSuccessfulOrder()
    {
        // Prevents "unlucky streaks" if you've completed a bunch of cups but keep failing to roll a satisfaction gain
        float bonusChance = failsSinceLastGain * 0.05f;
        float finalChance = Mathf.Clamp01(gainChance + bonusChance);

        if (Random.value <= finalChance)
        {
            GainSatisfaction();
            failsSinceLastGain = 0;
        }
        else
        {
            failsSinceLastGain++;
        }
    }

    public void OnFailedOrder()
    {
        failsSinceLastGain = 0;
        ModifySatisfaction(-failPenalty);
    }

    //=================== Internal Logic =================

    private void GainSatisfaction()
    {
        float baseGain = Random.Range(gainAmountRange.x, gainAmountRange.y);

        float multiplier = GetSoftCapMultiplier();

        float finalGain = baseGain * multiplier;

        ModifySatisfaction(finalGain);
    }

    // Satisfaction has a softcap. Gains have diminishing returns the closer you get to the softcap.
    private float GetSoftCapMultiplier()
    {
        if (currentSatisfaction <= satisfactionSoftCap)
            return 1f;

        float overAmount = currentSatisfaction - satisfactionSoftCap;
        float range = 100f - satisfactionSoftCap;

        float t = overAmount / range;

        return 1f - (t * t);
    }

    private void ModifySatisfaction(float amount)
    {
        currentSatisfaction += amount;
        ClampSatisfaction();
        NotifyUI();
    }

    private void ClampSatisfaction()
    {
        currentSatisfaction = Mathf.Clamp(currentSatisfaction, 0f, maxSatisfaction);
    }

    private void NotifyUI()
    {
        OnSatisfactionChanged?.Invoke(currentSatisfaction, maxSatisfaction);
    }

    //================= Upgrades =================

    public void IncreaseSoftcap(float amount)
    {
        satisfactionSoftCap = amount;
        NotifyUI();
    }
}
