using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class UpgradeUIManager : MonoBehaviour
{
    public static UpgradeUIManager Instance { get; private set; }

    [Header("UI References")]
    public RectTransform panel;
    public GameObject upgradeButtonPrefab;
    public Wallet playerWallet;

    [Header("UI Anims and Offset")]
    [SerializeField] private Vector3 buttonOffset;

    private Machine currentMachine;
    private List<GameObject> activeButtons = new();

    private bool ignoreClickThisFrame;

    private void Awake()
    {
        Instance = this;

        panel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!panel.gameObject.activeSelf)
            return;

        // Prevents upgrade UI from immediately closing after opening
        if (ignoreClickThisFrame)
        {
            ignoreClickThisFrame = false;
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // Check if we clicked on any active upgrade button
            bool clickedOnButton = false;
            foreach (var btn in activeButtons)
            {
                var buttonRect = btn.GetComponent<RectTransform>();
                if (RectTransformUtility.RectangleContainsScreenPoint(buttonRect, mousePos))
                {
                    clickedOnButton = true;
                    break;
                }
            }

            if (!clickedOnButton)
            {
                Close();
            }
        }
    }

    public void Open(Machine m_machine)
    {
        ignoreClickThisFrame = true;
        
        UpgradeEvents.OnUpgradeApplied -= HandleUpgradeApplied;
        UpgradeEvents.OnUpgradeApplied += HandleUpgradeApplied;

        currentMachine = m_machine;
        panel.gameObject.SetActive(true);

        Canvas canvas = panel.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.transform as RectTransform;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(m_machine.transform.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 localPos);

        panel.anchoredPosition = localPos + new Vector2(150f, 50f);

        BuildButtons();
    }

    public void Close()
    {
        Debug.Log("Closing Upgrade UI");
        
        UpgradeEvents.OnUpgradeApplied -= HandleUpgradeApplied;

        ClearButtons();
        panel.gameObject.SetActive(false);
        currentMachine = null;
    }

    private void BuildButtons()
    {
        if (currentMachine == null) return;

        ClearButtons();

        foreach (var state in currentMachine.GetUpgradeStates())
        {
            GameObject btn = Instantiate(upgradeButtonPrefab, panel);
            activeButtons.Add(btn);

            btn.GetComponent<UpgradeUIButton>()
               .Initialize(currentMachine, state);
        }
    }

    private void ClearButtons()
    {
        foreach (var btn in activeButtons)
            Destroy(btn);

        activeButtons.Clear();
    }

    public void OnUpgradeButtonClicked(Machine machine, UpgradeState state)
    {
        if (machine != currentMachine) return;
        if (state.IsMaxed) return;

        int cost = state.upgrade.GetCost(state.level);

        if (!playerWallet.Deduct(cost))
        {
            Debug.Log("Not enough cash!");
            return;
        }

        machine.ApplyUpgrade(state.upgrade);
        //Close();
    }

    private void HandleUpgradeApplied(Machine machine, Upgrade upgrade, int level)
    {
        if (machine != currentMachine) return;
        BuildButtons();
    }
}
