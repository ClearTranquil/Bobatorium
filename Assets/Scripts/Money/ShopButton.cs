using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShopItem item;
    [SerializeField] private GameObject hoverUIPrefab;

    [Header("UI Settings")]
    [SerializeField] private Vector3 popTextOffset = new Vector3(0f, 1f, 0f);

    private Canvas uiCanvas;
    private GameObject activeUI;
    private Camera cam;
    private Wallet wallet;

    private void Awake()
    {
        cam = Camera.main;

        wallet = FindFirstObjectByType<Wallet>();
        if (wallet == null) Debug.LogWarning("ShopButton: No Wallet found in scene.");

        uiCanvas = FindFirstObjectByType<Canvas>();
        if (uiCanvas == null) Debug.LogWarning("ShopButton: No Canvas found in scene.");
    }

    private void Update()
    {
        HandleHover();
        HandleClick();

        if (activeUI) UpdateUIPosition();
    }

    private void HandleHover()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                ShowHoverUI();
                return;
            }
        }

        HideHoverUI();
    }

    private void HandleClick()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                TryPurchase();
            }
        }
    }

    private void ShowHoverUI()
    {
        if (activeUI || !item) return;

        activeUI = Instantiate(hoverUIPrefab, uiCanvas.transform);

        int price = item.price;
        string priceInDollars = (price / 100f).ToString("F2");

        var text = activeUI.GetComponentInChildren<TMPro.TMP_Text>();

        text.text = $"{item.itemName}\n{item.itemDescription}\n${priceInDollars}";
    }

    private void HideHoverUI()
    {
        if (activeUI != null) Destroy(activeUI);
    }

    private void UpdateUIPosition()
    {
        Vector2 screenPos = cam.WorldToScreenPoint(transform.position + popTextOffset);

        activeUI.transform.position = screenPos;
    }

    private void TryPurchase()
    {
        if (!wallet.Deduct(item.price))
        {
            Debug.Log("Not enough money!");
            return;
        }

        Instantiate(item.prefabToSpawn, transform.position + item.spawnOffset, Quaternion.identity);
    }
}
