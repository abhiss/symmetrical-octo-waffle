using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using UnityEngine.Networking;


public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button server_button;
    [SerializeField] private Button host_button;
    [SerializeField] private Button client_button;
    [SerializeField] private Transform spawnLocation;

    private bool isMainScene;
    private GenLayout map;
    public void Start()
    {
        isMainScene = SceneManager.GetActiveScene().name == "MainScene";
        if(isMainScene){
            map = new GenLayout(Instantiate, gameObject, 0);
        }
		NetworkManager.Singleton.StartHost();

		var player = NetworkManager.LocalClient.PlayerObject;
        player.transform.position = map.PlayerSpawnLocation;

        if(isMainScene) {
            player.transform.position = map.PlayerSpawnLocation;
        } else {
            player.transform.position = spawnLocation.position;
        }
    }

        //private void Awake()
        //{
        //    if (!isMainScene) return;
        //    server_button.onClick.AddListener(() =>
        //    {
        //        NetworkManager.Singleton.StartServer();
        //    });

        //    host_button.onClick.AddListener(() =>
        //    {
        //        NetworkManager.Singleton.StartHost();
        //    });

        //    client_button.onClick.AddListener(() =>
        //    {
        //        NetworkManager.Singleton.StartClient();
        //    });

        //}

}

