using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleUI : MonoBehaviour
{
    [SerializeField] GameObject[] objectsToToggle;
    [SerializeField] KeyCode[] inputKeys;

    void Update()
    {
        for (int i = 0; i < objectsToToggle.Length; i++)
        {
            if (Input.GetKeyDown(inputKeys[i]))
            {
                bool activeState = objectsToToggle[i].activeInHierarchy;
                objectsToToggle[i].SetActive(!activeState);
            }
        }
    }
}
