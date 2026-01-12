using UnityEngine;

[CreateAssetMenu(menuName = "Sales/Modifiers/Crit")]
public class CritMod : SaleModifier
{
    [Range(0f, 1f)]
    public float critChance = 0.2f;
    public float critMultiplier = 2f;

    public override void Apply(SaleData sale)
    {
        if(Random.value <= critChance)
        {
            sale.finalValue = Mathf.RoundToInt(sale.finalValue * critMultiplier);
        }
    }
}
