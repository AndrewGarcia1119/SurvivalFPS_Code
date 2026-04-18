using UnityEngine;
using Steamworks.Data;
using Unity.Netcode;
using Steamworks;
using System;
using Netcode.Transports.Facepunch;
using ShooterSurvival.GameSystems;

public class GameLobbyManager : MonoBehaviour
{

    public static GameLobbyManager instance { get; private set; }
    private FacepunchTransport transport = null;
    public Lobby? currentLobby = null;

    public ulong hostId;

    public static readonly string GAMENAME = "ShooterSurvival", GAMEKEYNAME = "gametype", LOBBYNAMEKEY = "lobby_name";

    public static event Action onJoinedLobby;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

    }
    private void Start()
    {
        transport = GetComponent<FacepunchTransport>();
        SteamMatchmaking.OnLobbyCreated += OnMenuLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnMenuLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnMenuLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnMenuLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnMenuLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated += OnMenuLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += OnMenuLobbyGameJoinRequested;
    }


    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= OnMenuLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnMenuLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnMenuLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnMenuLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= OnMenuLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated -= OnMenuLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested -= OnMenuLobbyGameJoinRequested;

        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnServerStarted -= NetworkManager_Singleton_OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_Singleton_OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_Singleton_OnClientDisconnect;
    }

    void OnApplicationQuit()
    {
        Disconnected();
    }

    void Update()
    {
        SteamClient.RunCallbacks();
    }

    private async void OnMenuLobbyGameJoinRequested(Lobby lobby, SteamId id)
    {
        RoomEnter joinedLobby = await lobby.Join();
        if (joinedLobby != RoomEnter.Success)
        {
            Debug.LogWarning("Could not join lobby");
        }
        else
        {
            ConnectToLobby(lobby);
            onJoinedLobby?.Invoke();
            Debug.Log("Joined lobby");
        }
    }

    public void ConnectToLobby(Lobby lobby)
    {
        currentLobby = lobby;
        PlayerManager.instance.ConnectedAsClient();
    }

    private void OnMenuLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId id)
    {
        Debug.Log("Lobby created");
        //Debug.Log($"id: {id}, lobbyId {lobby.Id}");
    }

    private void OnMenuLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} left");
    }

    private void OnMenuLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log("member joined");
    }

    private void OnMenuLobbyInvite(Friend friend, Lobby lobby)
    {
        Debug.Log($"invite from {friend.Name}");
    }

    private void OnMenuLobbyEntered(Lobby lobby)
    {
        if (NetworkManager.Singleton.IsHost) return;
        StartClient(currentLobby.Value.Owner.Id);
    }

    private void OnMenuLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.Log("Could not create lobby");
        }
        else
        {
            if (lobby.MaxMembers > 1)
            {
                lobby.SetPublic();
                lobby.SetJoinable(true);
            }
            else
            {
                lobby.SetPrivate();
                lobby.SetJoinable(false);
            }
            lobby.SetGameServer(lobby.Owner.Id);
            lobby.SetData(GAMEKEYNAME, GAMENAME);
            lobby.SetData(LOBBYNAMEKEY, $"{lobby.Owner.Name}'s Lobby");
            Debug.Log($"Lobby created by {lobby.Owner.Name}");
        }
    }

    public async void StartHost(int maxMembers)
    {
        NetworkManager.Singleton.OnServerStarted += NetworkManager_Singleton_OnServerStarted;
        NetworkManager.Singleton.StartHost();
        PlayerManager.instance.myClientId = NetworkManager.Singleton.LocalClientId;
        currentLobby = await SteamMatchmaking.CreateLobbyAsync(maxMembers);
    }

    public void StartClient(SteamId steamId)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Singleton_OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Singleton_OnClientDisconnect;
        transport.targetSteamId = steamId;
        PlayerManager.instance.myClientId = NetworkManager.Singleton.LocalClientId;
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client has started");
        }
    }

    public void Disconnected()
    {
        if (currentLobby != null)
        {
            currentLobby?.Leave();
        }
        currentLobby = null;
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnServerStarted -= NetworkManager_Singleton_OnServerStarted;
        }
        else
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_Singleton_OnClientConnected;
        }
        NetworkManager.Singleton.Shutdown(true);
        PlayerManager.instance.Disconnected();
        Debug.Log("Disconnected via GLM");
    }
    private void NetworkManager_Singleton_OnClientConnected(ulong clientId)
    {
        //
    }

    private void NetworkManager_Singleton_OnClientDisconnect(ulong obj)
    {
        //
    }

    private void NetworkManager_Singleton_OnServerStarted()
    {
        Debug.Log("Host Started");
        PlayerManager.instance.HostCreated();
    }


}
