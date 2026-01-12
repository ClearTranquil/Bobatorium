using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaleProcessor : MonoBehaviour
{
    public Wallet wallet;
    public List<SaleModifier> saleModifiers;
    
    // Checks all applied sale modifiers and deposits the final amount earned into the player's wallet.
    public void ProcessSale(int baseValue)
    {
        SaleData sale = new SaleData(baseValue);

        foreach(var mod in saleModifiers)
        {
            mod.Apply(sale);
        }

        wallet.Deposit(sale.finalValue);
    }

    private void Update()
    {
        // debug sale test
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            //Debug.Log("Input detected");
            ProcessSale(5);
        }
    }
}
