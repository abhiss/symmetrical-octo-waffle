using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;

public class PauseMenu : MonoBehaviour
{
    public GameObject PauseMenuGameObject;
    private static bool _isPaused;
    public Button ResumeButton;
    public ToggleUI toggleScript; // Reference to the ToggleUI script

    public static bool IsPaused()
    {
        return _isPaused;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Pause(!_isPaused);
        }
    }

    public void Pause(bool pause)
    {
        if (pause)
        {
            PauseMenuGameObject.SetActive(true);
            Time.timeScale = 0;
            _isPaused = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            ResumeButton.Select();
        }
        else
        {
            PauseMenuGameObject.SetActive(false);
            Time.timeScale = 1;
            _isPaused = false;
        }
    }

    public void OnResumeButton()
    {
        Debug.Log("Resume button clicked.");
        TogglePause();
    }

    public void OnQuitButton()
    {
        Debug.Log("Quit button clicked.");
        Time.timeScale = 1;
        _isPaused = false;
        SceneManager.LoadScene("MainMenu"); // Replace "Main Menu" with the exact name of your main menu scene
    }

    public void TogglePause()
    {
        Pause(!_isPaused);
    }
}
