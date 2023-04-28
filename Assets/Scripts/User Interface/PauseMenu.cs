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
    public Toggle toggleScript; // Reference to the Toggle script

    public static bool IsPaused()
    {
        return _isPaused;
    }

    // Update is called once per frame
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
        TogglePause();
    }
    public void OnQuitButton()
    {
        Time.timeScale = 1;
        _isPaused = false;
        SceneManager.LoadScene("Main Menu"); // Replace "Main Menu" with the exact name of your main menu scene
    }

    public void TogglePause()
    {
        Pause(!_isPaused);
    }

}
