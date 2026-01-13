using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaleProcessor : MonoBehaviour
{
    public Wallet wallet;
    
    // Checks all applied sale modifiers and deposits the final amount earned into the player's wallet.
    public void ProcessSale(Cup cup)
    {
        SaleData sale = new SaleData(cup.GetBasePrice());

        foreach(var mod in cup.saleModifiers)
        {
            mod.Apply(sale);
        }

        wallet.Deposit(sale.finalValue);
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
