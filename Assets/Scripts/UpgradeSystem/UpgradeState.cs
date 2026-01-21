using UnityEngine;

[System.Serializable]
public class UpgradeState
{
    public Upgrade upgrade;
    public int level = 0;

    // Returns the value of this upgrade after its current stack
    public float CurrentValue
    {
        get
        {
            if (upgrade == null) return 0f;
            if (level <= 0) return 0f;
            if (upgrade.stackValues == null || upgrade.stackValues.Length == 0) return 0f;

            return upgrade.stackValues[level - 1];
        }
    }

    public bool IsMaxed
    {
        get
        {
            if (upgrade == null) return true;
            return level >= upgrade.maxStacks;
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
