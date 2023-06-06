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
        // For now, just log a message. When built, it will close the game.
        Debug.Log("Quit button clicked. This will close the game when built.");
        // Uncomment the following line to actually quit the game when built.
        // Application.Quit();
    }
}
