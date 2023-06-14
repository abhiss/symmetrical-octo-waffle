using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject JoinCodeUIGameObj;
    private TMP_Text JoinCodeTmp;
    // Start is called before the first frame update
    void Start()
    {
        JoinCodeTmp = JoinCodeUIGameObj.GetComponent<TMP_Text>();
        GlobalNetworkManager.Instance.OnJoinCodeChange((joincode) => {
            JoinCodeTmp.text = joincode;
        });
    }
}
