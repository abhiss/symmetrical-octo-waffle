using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    private CinematicPan cinematicPan;

    void Start()
    {
        cinematicPan = Camera.main.GetComponent<CinematicPan>();
    }

    public void OnStartButtonClick()
    {
        SceneManager.LoadScene("DevScene");
    }

    public void OnSettingsButtonClick()
    {
        cinematicPan.MoveToPoint(1);
    }

    public void OnBattlePassButtonClick()
    {
        cinematicPan.MoveToPoint(2);
    }

    public void OnLobbyButtonClick()
    {
        cinematicPan.MoveToPoint(3);
    }

    public void OnQuitButtonClick()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_WEBPLAYER
        Application.OpenURL(webplayerQuitURL);
        #else
        Application.Quit();
        #endif
    }
}
