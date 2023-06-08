using UnityEngine;
using TMPro;

public class MainPlayerDisplay : MonoBehaviour
{
    public TextMeshProUGUI playerNameText; // Assign this in inspector

    // Assuming you want to load the player name on start
    void Start()
    {
        LoadUsername();
    }

    public void LoadUsername()
    {
        if (PlayerPrefs.HasKey("username"))
        {
            // Set the text of your TextMeshProUGUI element
            playerNameText.text = PlayerPrefs.GetString("username");
        }
        else
        {
            Debug.LogError("No username found in PlayerPrefs");
        }
    }
}
