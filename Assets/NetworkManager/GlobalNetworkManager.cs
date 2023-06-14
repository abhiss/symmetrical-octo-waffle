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
using ProcGen;

public class GlobalNetworkManager : NetworkBehaviour
{
    public static GlobalNetworkManager Instance;
    public GameObject MapParentGO;

    private NetworkVariable<int> procGenSeed = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private bool isMainScene;
    private NetworkObject player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    private async void Start()
    {
        NetworkManager.OnClientConnectedCallback += OnClientConnected;
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
            if (!NetworkManager.Singleton.StartClient())
            {
                Debug.LogError("Failed to start client.");
            }
        }
        else
        {
            NetworkManager.Singleton.StartHost();
        }

        Debug.Log($"isserver={IsServer} isclient={IsClient} ishost={IsHost} isspawned={IsSpawned}");

        if (!isMainScene)
        {
            Debug.Log("not main scene");
            return;
        }
#endif
    }

    private void mapgen()
    {
        if (IsServer)
        {
            procGenSeed.Value = new System.Random().Next();
        }
        Debug.Log("in isserver");

        var config = new Config();
        config.Seed = procGenSeed.Value;
        config.Instantiate = Instantiate;
        config.GO = MapParentGO;
        // config.PremadeRooms = new List<PremadeRoom>();
        Debug.Log("Map seed: " + config.Seed);
        var generator = new Generator();
        generator.Generate(config);
        foreach (var obj in generator.NetworkObjects)
        {
            obj.Spawn();
        }
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

    void OnClientConnected(ulong clientid)
    {
        if (NetworkManager.LocalClientId != clientid) return;
        Debug.Log("Client: Connected to server");
        mapgen();
    }

    [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    public void DespawnGameObjectServerRpc(ulong networkObjectId)
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId].Despawn();
    }

    private int GetConnectedPlayersCount()
    {
        return 2;
    }
}
