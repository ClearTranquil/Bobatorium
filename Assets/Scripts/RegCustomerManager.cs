using UnityEngine;
using System.Collections.Generic;

public class RegCustomerManager : MonoBehaviour
{
    [Header("Convert Cutscene")]
    [SerializeField] private CustomerCutsceneController cutsceneController;

    [Header("Conversion")]
    [SerializeField] private int cupSoldBeforeConversion = 4;
    private int cupsSinceLastConversion = 0;
    [SerializeField] private float minimumSatisfaction = 50f;

    [Header("Conversion Scaling")]
    [SerializeField] private float maxConversionChance = 0.08f;

    [Header("Spawn Scaling")]
    [SerializeField] private float baseSpawnChance = 0.02f;
    [SerializeField] private float perRegularBonus = 0.005f;
    [SerializeField] private float maxSpawnChance = 0.10f;

    [Header("Regular Profiles")]
    [SerializeField] private List<CustomerProfile> regularProfiles = new();

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
            TryConvert(customer, customer.Profile);
        }
    }

    private void TryConvert(Customer customer,CustomerProfile profile)
    {
        if (cupsSinceLastConversion < cupSoldBeforeConversion) return;

        float sat = satisfaction.Current;
        if (sat <= minimumSatisfaction) return;

        float t = Mathf.InverseLerp(50f, 100f, sat);
        float chance = t * maxConversionChance;

        if (Random.value < chance)
        {
            StartConversionCutscene(customer, profile);
        }
    }

    private void StartConversionCutscene(Customer customer, CustomerProfile profile)
    {
        if (customer == null) return;

        cutsceneController.PlayCutscene(customer, () =>{FinalizeConversion(customer, profile);});
    }

    private void FinalizeConversion(Customer customer, CustomerProfile profile)
    {
        if (!regularProfiles.Contains(profile))
            regularProfiles.Add(profile);

        Debug.Log($"FINALIZE: {customer.name}");

        customer.SetRegular(true);

        cupsSinceLastConversion = 0;
    }

    public bool TryGetRegular(out CustomerProfile profile)
    {
        profile = null;

        if (regularProfiles.Count == 0)
            return false;

        float spawnChance = baseSpawnChance + (regularProfiles.Count * perRegularBonus);
        spawnChance = Mathf.Min(spawnChance, maxSpawnChance);

        if (Random.value > spawnChance)
            return false;

        profile = regularProfiles[Random.Range(0, regularProfiles.Count)];
        return true;
    }

    // For testing
    public void StartForceConversion(Customer customer)
    {
        if (customer == null)
            return;

        Debug.Log($"Customer: {customer}");
        Debug.Log($"Profile: {customer?.Profile}");
        Debug.Log($"CutsceneController: {cutsceneController}");

        cutsceneController.PlayCutscene(customer, () =>
        {
            FinalizeConversion(customer, customer.Profile);
        });
    }
}