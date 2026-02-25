using UnityEngine;

public class SaleData
{
    public int baseValue;
    public int finalValue;

    public bool didTip;
    public int tipAmount;

    public SaleData(int baseValue)
    {
        this.baseValue = baseValue;
        finalValue = baseValue;
    }
}
