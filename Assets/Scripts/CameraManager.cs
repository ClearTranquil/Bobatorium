using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera mainCam;
    [SerializeField] private CinemachineCamera breakRoomCam;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Image buttonImage;

    [Header("Sprites")]
    [SerializeField] private Sprite leftArrowSprite;
    [SerializeField] private Sprite rightArrowSprite;


    private bool isInBreakRoom = false;

    private void Start()
    {
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

    private void ToggleCamera()
    {
        isInBreakRoom = !isInBreakRoom;

        if (isInBreakRoom)
        {
            mainCam.Priority = 0;
            breakRoomCam.Priority = 10;

            buttonImage.sprite = rightArrowSprite;
        }
        else
        {
            mainCam.Priority = 10;
            breakRoomCam.Priority = 0;

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
