using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaleProcessor : MonoBehaviour
{
    public Wallet wallet;
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

        // Probability of tipping = remaining tip time normalized (0 to 1)
        float tipChance = customer.RemainingTipNormalized;

        // Roll random
        if (Random.value <= tipChance)
        {
            saleData.didTip = true;

            // Flat tip amount (unchanged)
            saleData.tipAmount = Mathf.RoundToInt(saleData.baseValue * 0.25f);
            saleData.finalValue += saleData.tipAmount;
        }
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
