using UnityEngine;

public enum MachineType
{
    BobaMachine,
    TeaMachine,
    CupSealer,
    CupDispenser,
    DeliveryTray
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrades/Upgrade")]
public class Upgrade : ScriptableObject
{
    [Tooltip("Internal upgrade name.")]
    public string upgradeID;
    [Tooltip("Name that is displayed to player.")]
    public string upgradeName;
    public string description;
    public Sprite icon;

    public MachineType machineType;

    public int baseCost = 10;

    [Tooltip("Each element is the value added to a stat, in order")]
    public float[] stackValues;
    public int maxLevel => stackValues != null ? stackValues.Length : 0;

    public int GetCost(int currentLevel)
    {
        return baseCost * (currentLevel + 1);
    }
}
