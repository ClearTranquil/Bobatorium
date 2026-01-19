using UnityEngine;

[System.Serializable]
public class UpgradeState
{
    public Upgrade upgrade;
    public int level = 0;

    public bool IsMaxed => level >= upgrade.maxStacks;

    // Returns the value of this upgrade after its current stack
    public float CurrentValue
    {
        get
        {
            if (level == 0) return 0;
            return upgrade.stackValues[level - 1];
        }
    }

    public UpgradeState(Upgrade upgrade)
    {
        this.upgrade = upgrade;
        level = 0;
    }

    public void Apply()
    {
        if (!IsMaxed)
        {
            level++;
        }
    }
}
