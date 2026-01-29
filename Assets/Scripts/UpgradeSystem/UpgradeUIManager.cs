using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class UpgradeUIManager : MonoBehaviour
{
    public static UpgradeUIManager Instance { get; private set; }

    [Header("UI References")]
    public RectTransform panel;
    public GameObject upgradeButtonPrefab;

    private Machine currentMachine;
    private List<GameObject> activeButtons = new();

    private void Awake()
    {
        Instance = this;

        panel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!panel.gameObject.activeSelf)
            return;

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
        UpgradeEvents.OnUpgradeApplied -= HandleUpgradeApplied;
        UpgradeEvents.OnUpgradeApplied += HandleUpgradeApplied;

        currentMachine = m_machine;
        panel.gameObject.SetActive(true);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(m_machine.transform.position);

        // Offset panel slightly so it doesn't overlap the machine
        panel.position = screenPos + new Vector3(150f, 50f, 0f);
        BuildButtons();
    }

    public void Close()
    {
        UpgradeEvents.OnUpgradeApplied -= HandleUpgradeApplied;

        ClearButtons();
        panel.gameObject.SetActive(false);
        currentMachine = null;
    }

    private void BuildButtons()
    {
        if (currentMachine == null) return;

        ClearButtons();
        
        foreach(var state in currentMachine.GetUpgradeStates())
        {
            GameObject btnToInit = Instantiate(upgradeButtonPrefab, panel);
            activeButtons.Add(btnToInit);
            btnToInit.GetComponent<UpgradeUIButton>().Initialize(currentMachine, state);

            var button = btnToInit.GetComponent<UpgradeUIButton>();
            button.Initialize(currentMachine, state);
        }
    }

    private void ClearButtons()
    {
        foreach (var btn in activeButtons)
            Destroy(btn);

        activeButtons.Clear();
    }

    private void HandleUpgradeApplied(Machine machine, Upgrade upgrade, int level)
    {
        if (machine != currentMachine) return;
        BuildButtons();
    }
}
