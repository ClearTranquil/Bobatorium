using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cup : MonoBehaviour, IInteractable
{
    private Camera mainCam;

    [Header("Pricing")]
    [SerializeField] private int basePrice = 1;
    public List<SaleModifier> saleModifiers;

    [Header("Physics")]
    [SerializeField] private float heldZDistance = 15f;
    private bool canBeGrabbed = true;

    [Header("Cup fill settings")]
    [SerializeField] private float maxTeaFill;
    [SerializeField] private float teaFillAmount;

    [Header("Current fill (debug only)")]
    [SerializeField] private int maxBoba;
    [SerializeField] private int bobaCount = 0;
    [SerializeField] private bool isSealed = false;

    [Header("Placeholder visuals")]
    [SerializeField] private GameObject emptyCup;
    [SerializeField] private GameObject cupWithBoba;
    [SerializeField] private GameObject cupWithTea;
    [SerializeField] private GameObject cupLid;
    [SerializeField] private GameObject straw;

    [Header("Position Snapping")]
    [SerializeField] private float followSmoothTime = 0.05f;
    private Vector3 velocity;
    private Vector3 desiredPosition;
    private CupSnapPoint heldSnapPoint;
    private CupSnapPoint currentSnapPoint;
    [SerializeField] private LayerMask snapMask;
    [SerializeField] private float snapMaxDistance = 100f;
    private int originalLayer;
    private int heldLayer = 7;

    [Header("Interface Variable Exposure")]
    public float TeaFillAmount => teaFillAmount;
    public bool TeaFill => IsTeaFull();
    public int BobaCount => bobaCount;
    public bool BobaFull => IsBobaFull();

    private void Awake()
    {
        mainCam = Camera.main;
        originalLayer = gameObject.layer;
    }

    public void Interact(PlayerControls player)
    {
        // If picked back up while it was snapped to a point, tell the snapPoint its been removed
        if(currentSnapPoint != null)
        {
            currentSnapPoint.Clear();
            ClearSnapPoint();
        }

        // When picked up, change obj's layer so the player can raycast through the cup
        gameObject.layer = heldLayer;
        transform.SetParent(null);
        player.PickUp(gameObject);
    }

    public void SetGrabEnabled(bool enabled)
    {
        canBeGrabbed = enabled;
    }

    public bool CanInteract(PlayerControls player)
    {
        return canBeGrabbed;
    }


    public void OnRelease(Vector3 releasePos)
    {
        // Revert to original layer so it can be interacted with again
        gameObject.layer = originalLayer;

        if (heldSnapPoint != null)
        {
            if (heldSnapPoint.TrySnap(this))
            {
                currentSnapPoint = heldSnapPoint;

                transform.position = currentSnapPoint.transform.position;
                velocity = Vector3.zero;
            }

            heldSnapPoint = null;
        }
    }

    public void OnHold()
    {
        // Follow the player's cursor while being held
        Vector3 mousePos = Mouse.current.position.ReadValue();
        desiredPosition = mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, heldZDistance));

        // Look for nearby snap points
        Ray ray = mainCam.ScreenPointToRay(mousePos);
        heldSnapPoint = FindBestSnapPoint(ray);

        if (heldSnapPoint)
            desiredPosition = heldSnapPoint.transform.position;

        // Smoothly move towards the desired position, cursor or snap point
        if (desiredPosition != Vector3.zero)
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, followSmoothTime);
        }
    }

    /*-----------------SNAP LOGIC------------------*/

    private CupSnapPoint FindBestSnapPoint(Ray ray)
    {
        // Raycast ignores the currently held cup so it can find snapPoints
        if (!Physics.Raycast(ray, out RaycastHit hit, snapMaxDistance, snapMask))
            return null;

        // First, check if we hit a machine
        Machine machine = hit.collider.GetComponentInParent<Machine>();
        if (machine != null)
            return machine.GetAvailableSnapPoint();

        // Fallback: check if we hit a standalone snap point
        CupSnapPoint snap = hit.collider.GetComponent<CupSnapPoint>();
        if (snap == null)
            snap = hit.collider.GetComponentInParent<CupSnapPoint>();

        if (snap != null && !snap.IsOccupied)
            return snap;

        return null;
    }

    public void RegisterSnapPoint(CupSnapPoint snapPoint)
    {
        currentSnapPoint = snapPoint;
    }

    public void ClearSnapPoint()
    {
        currentSnapPoint = null;
    }

    /*-----------------BOBA------------------*/
    public void AddBoba()
    {
        bobaCount++;

        if(bobaCount >= maxBoba)
        {
            UpdateVisuals();
        }
    }

    public bool IsBobaFull()
    {
        if (bobaCount >= maxBoba)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /*-----------------TEA-------------------*/

    // Checks if we're currently touching the tea machine's spout. 
    // Might replace this with a more elegant way to detect if tea is being poured later. 
    public void OnTriggerStay(Collider other)
    {
        TeaMachine machine = other.GetComponentInParent<TeaMachine>();
        if (machine && machine.IsPouring)
        {
            AddTea(machine.PourRate * Time.deltaTime);
        }
    }

    private void AddTea(float amount)
    {
        if (!isSealed)
        {
            if (teaFillAmount < maxTeaFill)
            {
                teaFillAmount = Mathf.Clamp(teaFillAmount + amount, 0f, maxTeaFill);
            }
            else
            {
                UpdateVisuals();
            }
        } else
        {
            Debug.Log("Can't fill, cup is sealed!");
        }
    }

    public bool IsTeaFull()
    {
        if (teaFillAmount >= maxTeaFill)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /*-----------------LID-------------------*/
    public void SealCup()
    {
        isSealed = true;

        UpdateVisuals();
    }

    public bool GetIsSealed()
    { return isSealed; }

    private void UpdateVisuals()
    {
        // This is where we'll make it look like liquid is being added to the cup
        if (IsTeaFull())
        {
            cupWithTea.SetActive(true);
            emptyCup.SetActive(false);
            cupWithBoba.SetActive(false);

        } else if (IsBobaFull())
        {
            cupWithBoba.SetActive(true);
            emptyCup.SetActive(false);
        }
        else
        {
            emptyCup.SetActive(true);
        }

        if (isSealed)
        {
            cupLid.SetActive(true);
        }
    }

    public void OnCupValiated()
    {
        if(straw)
            straw.SetActive(true);
    }

    public int GetBasePrice()
    {
        return basePrice;
    }
}
