using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SaleModifiers/Modifier Set")]
public class SaleModifierSet : ScriptableObject
{
    public List<SaleModifier> modifiers;
}
