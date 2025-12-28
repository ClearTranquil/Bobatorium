using UnityEngine;
using UnityEngine.InputSystem;

public class CupDispenser : MonoBehaviour
{
    [SerializeField] private Cup cupPrefab;
    [SerializeField] private float spawnOffsetY = 1f;
    private Cup activeCup;

    [SerializeField] private float heldCupZpos = 6f;

    void Update()
    {
        HandleMouseInput();
        HandleCupDrag();
    }

    private void HandleMouseInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if(Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if(hit.collider.gameObject == gameObject)
                {
                    Debug.Log("Dispenser clicked");
                    SpawnCup();
                }
            }
        }
    }

    private void SpawnCup()
    {
        if (activeCup != null)
            return;

        Vector3 spawnPos = transform.position + Vector3.up * spawnOffsetY;
        activeCup = Instantiate(cupPrefab, spawnPos, Quaternion.identity);
    }

    private void HandleCupDrag()
    {
        if (activeCup == null)
            return;

        // Only update cup position while holding LMouse
        if (Mouse.current.leftButton.isPressed)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            // Set cup's z pos to the default position
            Vector3 targetPos = ray.GetPoint(heldCupZpos);

            // Raycast to see if were hovering over a machine or tray. Snap to a set position on object
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.collider.CompareTag("Machine") || hit.collider.CompareTag("Tray"))
                {
                    Machine machine = hit.collider.GetComponent<Machine>();
                    if(machine != null && machine.snapPoint != null)
                    {
                        targetPos = machine.snapPoint.position;
                    }
                }
            }

            activeCup.transform.position = targetPos;
        }

        // Drop the cup when mouse released
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            HandleCupRelease();
        }
    }

    private void HandleCupRelease()
    {
        if (activeCup == null)
            return;

        // Check if cup is over delivery tray
        Collider[] hits = Physics.OverlapSphere(activeCup.transform.position, 0.2f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Tray"))
            {
                Debug.Log("Cup delivered!");
                Destroy(activeCup.gameObject);
                activeCup = null;
                return;
            }
        }

        // If not over tray, just drop cup in place for now
        activeCup = null;
    }
}
