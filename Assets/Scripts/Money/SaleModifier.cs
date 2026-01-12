using UnityEngine;

public abstract class SaleModifier : ScriptableObject
{
    public abstract void Apply(SaleData sale);
}
