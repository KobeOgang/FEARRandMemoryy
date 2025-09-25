using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;

    private bool isPaused = false;

    private void Update()
    {
        // Remove ESC handling - GameUIManager handles this now
        // ESC logic moved to GameUIManager
    }

    public void OpenPauseMenu()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseMenuPanel.SetActive(true);
    }

    public void ClosePauseMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseMenuPanel.SetActive(false);
    }

    // For UI button calls
    public void TogglePause()
    {
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.TogglePause();
        }
    }
}
