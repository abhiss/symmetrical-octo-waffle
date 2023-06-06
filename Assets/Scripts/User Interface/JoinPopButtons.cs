using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinPopButtons : MonoBehaviour
{
    public GameObject joinPopup;

    public void ClosePopup()
    {
        joinPopup.SetActive(false);
    }
}
