using UnityEngine;
using System;

public static class SaleEvents
{
    // Fires whenever a cup is sold
    public static Action<Cup, Customer> OnCupSold;

    // Fires when a cup is ready for pickup by a customer
    public static Action<Cup, Customer> OnCupReady;

    // Fires when a customer's wait time runs out
    public static Action<Customer> OnCustomerTimedOut;
}
