using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Shop Item")]
public class ShopItem : ScriptableObject
{
    public string itemName;
    [TextArea] 
    public string itemDescription;

    public GameObject prefabToSpawn;
    public int price;

    public Vector3 spawnOffset;
}
