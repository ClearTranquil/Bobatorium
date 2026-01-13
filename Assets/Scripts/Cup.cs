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
    private SnapPoints heldSnapPoint;
    private SnapPoints currentSnapPoint;
    [SerializeField] private LayerMask snapMask;
    [SerializeField] private float snapMaxDistance = 100f;
    private int originalLayer;
    private int heldLayer = 7;

    [Header("Cup fill settings")]
    [SerializeField] private float maxTeaFill;
    private float teaFillAmount;
    [SerializeField] private int maxBoba;
    private int bobaCount = 0;
    private bool isSealed = false;

    [Header("Placeholder visuals")]
    [SerializeField] private GameObject emptyCup;
    [SerializeField] private GameObject cupWithBoba;
    [SerializeField] private GameObject cupWithTea;
    [SerializeField] private GameObject cupLid;

    private void Awake()
    {
        mainCam = Camera.main;
        originalLayer = gameObject.layer;
    }

    public bool isBobaFull()
    {
        if(bobaCount >= maxBoba)
        {
            return true;
        } else { return false; }
    }

    public bool GetIsSealed()
        { return isSealed; }

    public bool isTeaFull()
    {
        if(teaFillAmount >= maxTeaFill)
        {
            return true;
        } else { return false; }
    }

    public void Interact(PlayerControls player)
    {
        // If picked back up while it was snapped to a point, tell the snapPoint its been removed
        if(currentSnapPoint != null)
        {
            currentSnapPoint.Clear();
            ClearSnapPoint();
        }

        // When picked up, change obj's layer so the player can ray cast through the cup
        gameObject.layer = heldLayer;
        transform.SetParent(null);
        player.PickUp(gameObject);
    }


    public void OnRelease(Vector3 releasePos)
    {
        //Debug.Log("Cup released");
        gameObject.layer = originalLayer;

        // Check if the cup should be snapping to a nearby snap point 
        if (heldSnapPoint != null)
        {
            heldSnapPoint.TrySnapCup(this);
            currentSnapPoint = heldSnapPoint;
            heldSnapPoint = null;
            return;
        }

        // Logic for cups not dropped in machine here
    }

    public void OnHold()
    {
        // While held, cup follows the player's cursor by default
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector3 targetPos = mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, heldZDistance));
        transform.position = targetPos;

        // While held, the cup looks for snap points. If one is nearby, snap to it to "preview" where it will be placed if the player lets go. 
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        // First, check if the obj being hovered over is a machine. Machine snap point behavior checks all available snap points and puts the cup in the first available one.
        if (Physics.Raycast(ray, out RaycastHit hit, snapMaxDistance))
        {
            Machine machine = hit.collider.GetComponent<Machine>();
            if (machine != null)
            {
                SnapPoints snap = machine.GetAvailableSnapPoint();
                if (snap != null)
                {
                    heldSnapPoint = snap;
                    transform.position = snap.transform.position;
                    return;
                }
            }
        }

        // Second, check for direct snap points that arent tied to a machine. 
        if (Physics.Raycast(ray, out RaycastHit snapHit, snapMaxDistance, snapMask))
        {
            SnapPoints snap = snapHit.collider.GetComponent<SnapPoints>();
            if (snap != null && !snap.IsOccupied)
            {
                heldSnapPoint = snap;
                transform.position = snap.transform.position;
                return;
            }
        }
        
        // Third, if theres no snap points nearby, just keep the object glued to the cursor.
        heldSnapPoint = null;
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

    /*-----------------TEA-------------------*/

    // Checks if we're currently touching the tea machine's spout
    public void OnTriggerStay(Collider other)
    {
        //Debug.Log("Cup is touching a spout");

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
                Debug.Log("Cup is " + teaFillAmount + " full");
            }
            else
            {
                Debug.Log("Cup is filled");
                UpdateVisuals();
            }
        } else
        {
            Debug.Log("Can't fill, cup is sealed!");
        }
    }

    /*-----------------LID-------------------*/
    public void SealCup()
    {
        isSealed = true;

        UpdateVisuals();
    }


    public void RegisterSnapPoint(SnapPoints snapPoint)
    {
        currentSnapPoint = snapPoint;
    }

    public void ClearSnapPoint()
    {
        currentSnapPoint = null;
    }

    private void UpdateVisuals()
    {
        // This is where we'll make it look like liquid is being added to the cup
        if (isTeaFull())
        {
            cupWithTea.SetActive(true);
            emptyCup.SetActive(false);
            cupWithBoba.SetActive(false);

        } else if (isBobaFull())
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

    public int GetBasePrice()
    {
        return basePrice;
    }
}
