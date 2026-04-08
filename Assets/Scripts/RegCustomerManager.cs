using UnityEngine;
using System.Collections.Generic;

public class RegCustomerManager : MonoBehaviour
{
    [Header("Conversion")]
    [SerializeField] private int cupSoldBeforeConversion = 4;
    private int cupsSinceLastConversion = 0;
    [SerializeField] private float minimumSatisfaction = 50f;

    [Header("Conversion Scaling")]
    // Should max out at 8% with full upgrades and 100% satisfaction
    [SerializeField] private float maxConversionChance = 0.08f;

    [Header("Spawn Scaling")]
    [SerializeField] private float baseSpawnChance = 0.02f;
    [SerializeField] private float perRegularBonus = 0.005f;
    [SerializeField] private float maxSpawnChance = 0.10f;

    private List<Customer> regulars = new List<Customer>();

    private CustomerSatisfaction satisfaction;

    private void Awake()
    {
        satisfaction = FindFirstObjectByType<CustomerSatisfaction>();
    }

    private void OnEnable()
    {
        SaleEvents.OnCupSold += OnCupSold;
    }

    private void OnDisable()
    {
        SaleEvents.OnCupSold -= OnCupSold;
    }

    private void OnCupSold(Cup cup, Customer customer)
    {
        if (!customer.WasServedInTime) return;

        cupsSinceLastConversion++;

        if (!customer.IsRegular)
        {
            TryConvert(customer);
        }
    }

    private void TryConvert(Customer customer)
    {
        // Only try converting if enough cups have been sold since the last conversion
        // This prevents back to back conversions, making it rarer
        if (cupsSinceLastConversion < cupSoldBeforeConversion) return;

        float sat = satisfaction.Current;

        if (sat <= minimumSatisfaction) return;

        // Scale from 50% satisfaction to 100% satisfaction
        float t = Mathf.InverseLerp(50f, 100f, sat);
        float chance = t * maxConversionChance;

        if(Random.value  < chance)
        {
            ConvertToRegular(customer);
            cupsSinceLastConversion = 0;
        }
    }

    private void ConvertToRegular(Customer customer)
    {
        regulars.Add(customer);
        customer.SetRegular(true);

        Debug.Log("Converted to regular. Total: " + regulars.Count);
    }

    public bool TryGetRegular(out Customer regular)
    {
        regular = null;

        if (regulars.Count == 0) return false;

        float spawnChance = baseSpawnChance + (regulars.Count * perRegularBonus);
        spawnChance = Mathf.Min(spawnChance, maxSpawnChance);

        if (Random.value > spawnChance) return false;

        regular = regulars[Random.Range(0, regulars.Count)];
        return true;
    }
}
