using UnityEngine;

public class Cup : MonoBehaviour
{
    private int bobaCount = 0;
    private float teaFill;
    private bool isSealed;

    private int GetBobaCount()
        { return bobaCount; }

    private float GetTeaFill()
        { return teaFill; }

    private bool GetIsSealed()
        { return isSealed; }
}
