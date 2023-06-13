using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JoinCodeDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI joinCodeText;

    private void Update()
    {
        // Assuming that GlobalNetworkManager.Instance.JoinCode is accessible and it's a string.
        // If it's not a string, you need to convert it to a string.
        joinCodeText.text = GlobalNetworkManager.Instance.JoinCode;
    }
}
