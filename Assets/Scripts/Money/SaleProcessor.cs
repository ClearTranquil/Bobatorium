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

        bool isRegular = customer != null && customer.IsRegular;

        // Gather all modifiers
        List<SaleModifier> allMods = new List<SaleModifier>();

        if (cup != null)
            allMods.AddRange(cup.saleModifiers);

        if (customer != null)
            allMods.AddRange(customer.SaleModifiers);

        // Apply all modifiers
        for (int i = 0; i < allMods.Count; i++)
        {
            allMods[i].Apply(sale, customer);
        }

        // Tip logic happens AFTER modifiers
        ProcessTip(sale, customer, cup, isRegular);

        wallet.Deposit(sale.finalValue);

        LastSale = sale;
    }

    private void ProcessTip(SaleData saleData, Customer customer, Cup cup, bool isRegular)
    {
        if (customer == null)
            return;

        float satisfactionNormalized = satisfaction.CurrentNormalized;

        float tipChance = Mathf.Lerp(0.2f, 0.95f, satisfactionNormalized);

        // If a modifier already forced a tip (regulars, buffs, etc.), skip roll
        bool guaranteedTip = customer != null && customer.IsRegular;

        if (guaranteedTip)
        {
            saleData.didTip = true;
        }
        else
        {
            if (Random.value > tipChance)
                return;

            saleData.didTip = true;
        }

        float multiplier = saleData.tipMultiplier;

        // Jackpot chance (5%)
        if (Random.value <= 0.05f)
        {
            saleData.tipAmount = Mathf.RoundToInt(5f * multiplier);
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

            saleData.tipAmount = Mathf.RoundToInt(saleData.baseValue * tipPercent * multiplier);
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
