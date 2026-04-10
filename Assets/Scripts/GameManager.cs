using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Pausing")]
    public static bool IsPaused { get; private set; }

    public static void Pause()
    {
        Time.timeScale = 0f;
        IsPaused = true;
    }

    public static void Resume()
    {
        Time.timeScale = 1f;
        IsPaused = false;
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            Resume();
        } else
        {
            Pause();
        }
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }
}
