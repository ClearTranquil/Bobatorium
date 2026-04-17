using UnityEngine;

[CreateAssetMenu(menuName = "NPC/Customer Profile")]
public class CustomerProfile : ScriptableObject
{
    public string customerName;
    public string bark;

    [Header("Visuals")]
    public Sprite head;
    public Sprite body;

    [Header("Stats")]
    [Range(0f, 1f)] public float baseTipChance = 0.1f;
    public float tipTime = 10f;
}
