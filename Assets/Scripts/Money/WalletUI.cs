using UnityEngine;
using TMPro;
using System.Collections;
using UnityEditor.UI;

public class WalletUI : MonoBehaviour
{
    public Wallet wallet;
    public TMP_Text balanceText;

    [SerializeField] private float rollSpeed = 200f;
    private int displayedBalance;
    private int targetBalance;
    private float rollTimer;

    [SerializeField] private TMP_Text saleTextPrefab;
    [SerializeField] private RectTransform saleTextSpawnPoint;
    [SerializeField] private Transform saleTextRoot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!wallet)
        {
            Debug.LogError("Wallet not found");
            return;
        }

        displayedBalance = wallet.balance;
        targetBalance = wallet.balance;

        UpdateBalanceText(displayedBalance);

        wallet.OnMoneyChanged += OnWalletChanged;
        SaleEvents.OnCupSold += OnSaleMade;
    }

    private void Update()
    {
        if (displayedBalance == targetBalance)
            return;

        rollTimer += Time.deltaTime;

        float secondsPerCent = 1f / rollSpeed;

        while (rollTimer >= secondsPerCent && displayedBalance != targetBalance)
        {
            rollTimer -= secondsPerCent;

            if (displayedBalance < targetBalance)
                displayedBalance++;
            else
                displayedBalance--;

            UpdateBalanceText(displayedBalance);
        }
    }


    private void OnDestroy()
    {
        wallet.OnMoneyChanged -= UpdateBalance;
        SaleEvents.OnCupSold -= OnSaleMade;
    }

    public void UpdateBalance(int amount)
    {
        //Debug.Log("Trying to update balance...");
        balanceText.text = "$" + (amount / 100f).ToString("F2");
    }

    private void OnWalletChanged(int newAmount)
    {
        targetBalance = newAmount;
    }

    private void UpdateBalanceText(int amount)
    {
        balanceText.text = "$" + (amount / 100f).ToString("F2");
    }

    private void OnSaleMade(Cup cup, Customer customer)
    {
        var sale = SaleProcessor.LastSale;

        if (sale == null)
            return;

        int saleAmount = sale.finalValue - sale.tipAmount;

        if (sale.didTip)
        {
            SpawnSaleText($"+${saleAmount / 100f:F2} <color=#00FF00>+${sale.tipAmount / 100f:F2} tip</color>", Color.white);
        } else
        {
            SpawnSaleText($"+${saleAmount / 100f:F2}", Color.white);
        }
    }

    private void SpawnSaleText(string text, Color color)
    {
        TMP_Text popup = Instantiate(saleTextPrefab, saleTextSpawnPoint);

        popup.text = text;
        popup.color = color;
    }

    //private IEnumerator AnimateSaleText(TMP_Text text)
    //{
    //    float duration = 1.5f;
    //    float timer = 0f;

    //    RectTransform rect = text.rectTransform;
    //    Color startColor = text.color;

    //    Vector3 startPos = rect.anchoredPosition;
    //    Vector3 endPos = startPos + new Vector3(0, 60f, 0);

    //    while (timer < duration)
    //    {
    //        timer += Time.deltaTime;
    //        float t = timer / duration;

    //        // Move upward
    //        rect.anchoredPosition = Vector3.Lerp(startPos, endPos, t);

    //        // Fade out
    //        Color c = startColor;
    //        c.a = Mathf.Lerp(1f, 0f, t);
    //        text.color = c;

    //        yield return null;
    //    }

    //    Destroy(text.gameObject);
    //}
}
