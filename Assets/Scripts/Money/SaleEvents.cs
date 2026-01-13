using UnityEngine;
using System;

public static class SaleEvents
{
    // Fires whenever a cup is sold
    public static Action<Cup> OnCupSold;
}
