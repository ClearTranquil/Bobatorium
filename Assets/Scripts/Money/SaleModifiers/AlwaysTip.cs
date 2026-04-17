using UnityEngine;

[CreateAssetMenu(menuName = "SaleModifiers/Always Tip")]
public class AlwaysTip : SaleModifier
{
    public override void Apply(SaleData sale, Customer customer)
    {
        if (customer == null) return;

        sale.didTip = true;
    }
}
