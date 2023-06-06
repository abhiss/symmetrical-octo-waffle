using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using UnityEngine.Networking;


public class GlobalNetworkManager : NetworkBehaviour
{

    private NetworkVariable<Vector3> spawnLocation = new NetworkVariable<Vector3>();

    private bool isMainScene;
    private GenLayout map;
    private NetworkObject player;

    void Start()
    {
        // automatically start as client if detected that instance is a parallelsync clone
        if (Application.dataPath.Contains("clone"))
        {
            NetworkManager.Singleton.StartClient();
        }
        else
        {
            NetworkManager.Singleton.StartHost();
        }

        Debug.Log($"isserver={IsServer} isclient={IsClient} ishost={IsHost} isspawned={IsSpawned}");

        isMainScene = SceneManager.GetActiveScene().name == "MainScene";
        if (!isMainScene)
        {
            Debug.Log("not main scene");
            player.transform.position = spawnLocation.Value;
            return;
        }
        if (IsServer)
        {
            Debug.Log("in isserver");
            map = new GenLayout(Instantiate, gameObject, 0);
            Debug.Log("map spawnlocation: " + map.PlayerSpawnLocation);
            spawnLocation.Value = map.PlayerSpawnLocation;
        }
        if (IsClient)
        {
            player = NetworkManager.LocalClient.PlayerObject;
            player.transform.position = spawnLocation.Value;
        }



    }

    // Should only be called on the current client. 
    // id is clientid (only makes sense to use on server). 
    private void OnClientConnected(ulong id) {

    }
}