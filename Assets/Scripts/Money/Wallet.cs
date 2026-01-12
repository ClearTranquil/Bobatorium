using System;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    public int balance { get; private set; }

    public event Action<int> OnMoneyChanged;

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
