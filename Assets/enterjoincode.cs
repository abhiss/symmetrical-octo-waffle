using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class enterjoincode : MonoBehaviour
{
    public TMP_InputField textbox;
    // Start is called before the first frame update
    void Start()
    {
        textbox.onSubmit.AddListener((string s) =>
        {
            Debug.Log(textbox.text);
            GlobalNetworkManager.Instance.JoinRelay(s);
        });
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
