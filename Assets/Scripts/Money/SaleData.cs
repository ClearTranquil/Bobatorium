public class SaleData
{
    public int baseValue;
    public int finalValue;

    public bool didTip;
    public bool tipResolved;

    public int tipAmount;
    public float tipMultiplier = 1f;

    public float tipChance; // optional debug

    public SaleData(int baseValue)
    {
        this.baseValue = baseValue;
        finalValue = baseValue;
    }
}