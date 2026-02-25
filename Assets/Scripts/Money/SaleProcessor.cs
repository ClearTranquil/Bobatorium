using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaleProcessor : MonoBehaviour
{
    public Wallet wallet;
    
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
    }

    private void ProcessTip(SaleData saleData, Customer customer, Cup cup)
    {
        float tipChance = 0f;

        if (customer != null)
            tipChance += customer.GetTipChance();

        if (cup != null)
            tipChance += cup.GetTipBonus();

        if (Random.value <= tipChance)
        {
            saleData.didTip = true;

            // Current tip formula is 25%, we'll probably have modifiers for this later
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
