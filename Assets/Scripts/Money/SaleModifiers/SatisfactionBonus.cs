using UnityEngine;

[CreateAssetMenu(menuName = "SaleModifiers/Satisfaction Bonus")]
public class SatisfactionBonus : SaleModifier
{
    public float bonus = 3f;

    public override void Apply(SaleData sale, Customer customer)
    {
        if (customer == null || !customer.IsRegular) return;

        var sat = FindFirstObjectByType<CustomerSatisfaction>();
        if (sat != null)
            sat.GainFixedSatisfaction(bonus);
    }
}
