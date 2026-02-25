using UnityEngine;

public class Customer : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float baseTipChance = 0.1f;

    public float GetTipChance()
    {
        return baseTipChance;
    }
}
