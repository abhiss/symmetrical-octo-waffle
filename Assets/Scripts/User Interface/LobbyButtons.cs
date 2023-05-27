using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyButtons : MonoBehaviour
{
    public CinematicPan cinematicPan;
    public CanvasManager canvasManager;
    public TMP_InputField usernameInputField;
    public GameObject joinPopup;

    public void OnHostButtonClick()
    {
        SaveUsername();
        SceneManager.LoadScene("DevScene");
    }

    public void OnJoinButtonClick()
    {
        SaveUsername();
        joinPopup.SetActive(true);
    }

    public void OnBackButtonClick()
    {
        cinematicPan.MoveToPoint(0);
        canvasManager.ActivateCanvas(0);
    }

    private void Start()
    {
        LoadUsername();
    }

    public void SaveUsername()
    {
        PlayerPrefs.SetString("username", usernameInputField.text);
    }

    public void LoadUsername()
    {
        if (PlayerPrefs.HasKey("username"))
        {
            usernameInputField.text = PlayerPrefs.GetString("username");
        }
    }
    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("username");
    }
    private void OnDisable()
    {
        PlayerPrefs.DeleteKey("username");
    }

}
