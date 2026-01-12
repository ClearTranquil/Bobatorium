using UnityEngine;
using TMPro;

public class WalletUI : MonoBehaviour
{
    public Wallet wallet;
    public TMP_Text balanceText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!wallet)
        {
            Debug.LogError("Wallet not found");
            return;
        }

        Debug.Log("Subscribing to wallet events");
        
        UpdateBalance(wallet.balance);

        // listen for the transaction event
        wallet.OnMoneyChanged += UpdateBalance;
    }

    private void OnDestroy()
    {
        wallet.OnMoneyChanged -= UpdateBalance;
    }

    public void UpdateBalance(int amount)
    {
        Debug.Log("Trying to update balance...");
        balanceText.text = $"${amount}";
    }
}
