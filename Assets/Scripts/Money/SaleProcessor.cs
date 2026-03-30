using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaleProcessor : MonoBehaviour
{
    public Wallet wallet;
    public CustomerSatisfaction satisfaction;
    public static SaleData LastSale;

    // Checks all applied sale modifiers and deposits the final amount earned into the player's wallet.
    public void ProcessSale(Cup cup, Customer customer)
    {
        SaleData sale = new SaleData(cup.GetBasePrice());

        foreach(var mod in cup.saleModifiers)
        {
            mod.Apply(sale);
        }

        ProcessTip(sale, customer, cup);

        wallet.Deposit(sale.finalValue);

        LastSale = sale;
    }

    private void ProcessTip(SaleData saleData, Customer customer, Cup cup)
    {
        if (customer == null)
            return;

        float satisfactionNormalized = satisfaction.CurrentNormalized;

        float tipChance = Mathf.Lerp(0.2f, 0.95f, satisfactionNormalized);

        if (Random.value > tipChance)
            return;

        saleData.didTip = true;

        // Jackpot chance (5%)
        if (Random.value <= 0.05f)
        {
            saleData.tipAmount = 5; // flat $5
        }
        else
        {
            float roll = Random.value;

            float tipPercent;
            if (roll < 0.33f)
                tipPercent = 0.15f;
            else if (roll < 0.66f)
                tipPercent = 0.20f;
            else
                tipPercent = 0.30f;

            saleData.tipAmount = Mathf.RoundToInt(saleData.baseValue * tipPercent);
        }

        saleData.finalValue += saleData.tipAmount;
    }

    private void OnEnable()
    {
        SaleEvents.OnCupSold += ProcessSale;
    }

    private void OnDisable()
    {
        SaleEvents.OnCupSold -= ProcessSale;
    }

}
