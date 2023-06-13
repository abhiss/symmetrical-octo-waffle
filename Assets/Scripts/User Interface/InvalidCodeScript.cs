using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InvalidCodeScript : MonoBehaviour
{
    [SerializeField] private GameObject invalidCodeText;

    // Update is called once per frame
    void Update()
    {
        if(false) //add condition here
        {
            invalidCodeText.SetActive(true);
        }
        else
        {
            invalidCodeText.SetActive(false);
        }
    }
}
