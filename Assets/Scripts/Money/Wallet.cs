using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Wallet : MonoBehaviour
{
    public int balance { get; private set; }

    public event Action<int> OnMoneyChanged;

    private void Update()
    {
        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            Deposit(50);
            Debug.Log("Debug: 50 dollars added to wallet");
        }
    }

    public void Deposit(int amount)
    {
        balance += amount;
        OnMoneyChanged?.Invoke(balance);
    }

    public bool CanAfford(int cost)
    {
        return balance >= cost;
    }

    public bool Deduct(int amount)
    {
        if (!CanAfford(amount))
            return false;

        balance -= amount;
        OnMoneyChanged?.Invoke(balance);
        return true;
    }
}
