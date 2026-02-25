using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    [Header("References")]
    private Transform cameraTransform;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Image buttonImage;

    [Header("Sprites")]
    [SerializeField] private Sprite leftArrowSprite;
    [SerializeField] private Sprite rightArrowSprite;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 180f;

    private Quaternion mainRotation;
    private Quaternion breakRoomRotation;
    private Quaternion targetRotation;

    private bool isInBreakRoom = false;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        mainRotation = Quaternion.Euler(10f, 0f, 0f);
        breakRoomRotation = Quaternion.Euler(10f, -90f, 0f);

        targetRotation = mainRotation;
        buttonImage.sprite = leftArrowSprite;

        toggleButton.onClick.AddListener(ToggleCamera);
    }

    private void OnEnable()
    {
        Employee.OnEdgeScreenSwitchRequest += HandleEdgeSwitch;
    }

    private void OnDisable()
    {
        Employee.OnEdgeScreenSwitchRequest -= HandleEdgeSwitch;
    }

    private void Update()
    {
        cameraTransform.rotation = Quaternion.RotateTowards(cameraTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void ToggleCamera()
    {
        // Toggle between the two states 
        isInBreakRoom = !isInBreakRoom;

        if (isInBreakRoom)
        {
            targetRotation = breakRoomRotation;
            buttonImage.sprite = rightArrowSprite;
        } else
        {
            targetRotation = mainRotation;
            buttonImage.sprite = leftArrowSprite;
        }
    }

    private void HandleEdgeSwitch(bool fromLeftSide)
    {
        // When in main room and employee is moved to the left side of screen
        if (!isInBreakRoom && fromLeftSide)
        {
            ToggleCamera();
        }
        // And vice versa 
        else if (isInBreakRoom && !fromLeftSide)
        {
            ToggleCamera();
        }
    }
}
