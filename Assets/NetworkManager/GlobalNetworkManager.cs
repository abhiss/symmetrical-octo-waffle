using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using UnityEngine.Networking;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class GlobalNetworkManager : NetworkBehaviour
{
    public static GlobalNetworkManager Instance;
    private NetworkVariable<Vector3> spawnLocation = new NetworkVariable<Vector3>();

    private bool isMainScene;
    private GenLayout map;
    private NetworkObject player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(transform.gameObject);

    }
    private async void Start()

    {
        NetworkManager.OnClientConnectedCallback += (ulong id) =>
        {
            Debug.Log("ON CLIENT CONNECTED.");
        };
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        isMainScene = SceneManager.GetActiveScene().name == "MainScene";
        var isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";

        if(isMainMenu) return;
#if UNITY_EDITOR
        Debug.Log("In unity editor.");
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
            //spawnLocation.Value = map.PlayerSpawnLocation;
        }
        if (IsClient)
        {
            player = NetworkManager.LocalClient.PlayerObject;
            player.transform.position = spawnLocation.Value;
        }
#endif
    }

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(7);
            string joincode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Joincode: " + joincode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
            NetworkManager.Singleton.StartHost();

        } catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinRelay(string joincode)
    {
        SceneManager.LoadScene("MainScene");
        try
        {
            Debug.Log("Joining game with code: " + joincode);
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joincode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAlloc.RelayServer.IpV4,
                (ushort)joinAlloc.RelayServer.Port,
                joinAlloc.AllocationIdBytes,
                joinAlloc.Key,
                joinAlloc.ConnectionData,
                joinAlloc.HostConnectionData
            );
            NetworkManager.Singleton.StartClient();
        } catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private int GetConnectedPlayersCount()
    {
        return 2;
    }
}
