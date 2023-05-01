using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using UnityEngine.Networking;


public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button server_button;
    [SerializeField] private Button host_button;
    [SerializeField] private Button client_button;


    private void Start()
    {
        NetworkManager.Singleton.StartHost();        // Starts the NetworkManager as both a server and a client (that is, has local client)

    }

    private void Awake()
    {
        server_button.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });

        host_button.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });

        client_button.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });

    }

}

