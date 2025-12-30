using UnityEngine;

public class Wallet : MonoBehaviour
{
    private int balance;

    public void Deduct(int m_deduction)
    {
        balance = balance - m_deduction; 
    }

    public int GetBalance() {  return balance; }
}
