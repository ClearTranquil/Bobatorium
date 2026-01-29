using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIButton : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Button button;
    [SerializeField] private Sprite icon;

    private Machine machine;
    private Upgrade upgrade;
    private UpgradeState state;

    public void Initialize(Machine m_machine,  UpgradeState m_state)
    {
        machine = m_machine;
        state = m_state;
        upgrade = m_state.upgrade;

        Refresh();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    public void Refresh()
    {
        titleText.text = upgrade.upgradeName;
        levelText.text = $"Lv {state.level}/{upgrade.maxLevel}";
        icon = upgrade.icon;
        button.interactable = !state.IsMaxed;
    }

    private void OnClicked()
    {
        machine.ApplyUpgrade(upgrade);
        
        UpgradeUIManager.Instance.Close();
    }
}
