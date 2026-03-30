using UnityEngine;
using TMPro;

public class SatisfactionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private void OnEnable()
    {
        CustomerSatisfaction.OnSatisfactionChanged += UpdateUI;
    }

    private void OnDisable()
    {
        CustomerSatisfaction.OnSatisfactionChanged -= UpdateUI;
    }

    private void UpdateUI(float current, float max)
    {
        text.text = $"Satisfaction: {Mathf.RoundToInt(current)}%";
    }
}
