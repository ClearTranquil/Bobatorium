using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Shop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private ShopItem[] shopItems;

    private Wallet wallet;

    private void Awake()
    {
        wallet = FindFirstObjectByType<Wallet>();
        if (!wallet) Debug.LogWarning("ClipboardShopManager: No Wallet found in scene.");
    }

    private void Start()
    {
        PopulateShop();
    }

    private void PopulateShop()
    {
        foreach(var item in shopItems)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
            TMP_Text text = buttonObj.GetComponentInChildren<TMP_Text>();
            text.text = $"{item.itemName}\n{item.itemDescription}\n${(item.price / 100f):F2}";

            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => TryPurchase(item));
        }
    }

    private void TryPurchase(ShopItem item)
    {
        if (!wallet.Deduct(item.price))
        {
            Debug.Log("Not enough money!");
            return;
        }

        Instantiate(item.prefabToSpawn, transform.position + item.spawnOffset, Quaternion.identity);
        Debug.Log($"Purchased {item.itemName}");
    }
}
