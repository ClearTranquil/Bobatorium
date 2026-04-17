using UnityEngine;

[CreateAssetMenu(menuName = "SaleModifiers/Tip Multiplier")]
public class TipMultiplier : SaleModifier
{
    public float multiplier = 2f;
    public override void Apply(SaleData sale, Customer customer)
    {
        sale.tipMultiplier *= multiplier;
    }
}
