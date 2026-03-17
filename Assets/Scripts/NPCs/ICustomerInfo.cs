using UnityEngine;

public interface ICustomerInfo
{
    float BaseTipChance { get; }
    Transform CupSlot { get; }
    float TipTime { get; }
    bool CanTip { get; }
}
