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

    // Backup pause method specifically 
    public void PauseForCutscene()
    {
        Time.timeScale = 0;
    }

    public void UnpauseCutscene()
    {
        Time.timeScale = 1;
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }
}
