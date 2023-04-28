using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toggle : MonoBehaviour
{
    [SerializeField] GameObject[] objectsToToggle;
    [SerializeField] KeyCode[] inputKeys;
    [SerializeField] PauseMenu pauseMenuScript; // Reference to the PauseMenu script

    void Update()
    {
        for (int i = 0; i < objectsToToggle.Length; i++)
        {
            if (Input.GetKeyDown(inputKeys[i]))
            {
                bool activeState = objectsToToggle[i].activeInHierarchy;
                objectsToToggle[i].SetActive(!activeState);

                // Call the TogglePause method from the PauseMenu script
                if (objectsToToggle[i].name == "Pause Menu") // Make sure to match the name of your PauseMenu GameObject
                {
                    pauseMenuScript.TogglePause();
                }
            }
        }
    }


    // Add this new method to the Toggle script
    public void TogglePauseMenu()
    {
        for (int i = 0; i < objectsToToggle.Length; i++)
        {
            if (objectsToToggle[i].name == "Pause Menu") // Make sure to match the name of your PauseMenu GameObject
            {
                bool activeState = objectsToToggle[i].activeInHierarchy;
                objectsToToggle[i].SetActive(!activeState);
                pauseMenuScript.Pause(!activeState);
                break;
            }
        }
    }
}
