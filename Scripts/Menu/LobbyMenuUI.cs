using UnityEngine;
using Netcode.Transports.Facepunch;
using Steamworks.Data;
using Unity.Netcode;
using Steamworks;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class LobbyMenuUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text lobbyName = null;
    [SerializeField]
    [Tooltip("list of player UI for host")]
    private TMP_Text[] playerList = null;

    [SerializeField]
    private TMP_Text lobbyName_join = null;
    [SerializeField]
    [Tooltip("list of player UI for joining")]
    private TMP_Text[] playerList_join = null;


    [SerializeField]
    private int maxLobbiesInPage = 15;
    [SerializeField]
    [Tooltip("prefab for UI that shows info a singular lobby when searching")]
    private LobbyInfoUI lobbyInfoPrefab = null;
    [SerializeField]
    [Tooltip("This should be the parent of any created lobby info prefab")]
    private GameObject lobbyInfoContainer = null;
    [SerializeField]
    [Tooltip("In Lobby - Joined Object (in scene hierarchy, not prefab)")]
    private GameObject joinLobbyObject;
    [SerializeField]
    [Tooltip("Lobby Search Object (in scene hierarchy, not prefab)")]
    private GameObject lobbySearchObject;

    private List<Lobby> lobbies = new();
    private List<Lobby> filteredLobbies = new();
    private List<LobbyInfoUI> lobbyInfoList = new();
    LobbyQuery lobbyQuery = new();

    #region Script Lifecycle
    private void Start()
    {
        SteamMatchmaking.OnLobbyMemberJoined += OnMenuLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnMenuLobbyMemberLeave;
        SteamMatchmaking.OnLobbyGameCreated += OnMenuLobbyGameCreated;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnMenuLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyEntered += OnMenuLobbyEntered;
        lobbyQuery = lobbyQuery.WithKeyValue(GameLobbyManager.GAMEKEYNAME, GameLobbyManager.GAMENAME);
    }

    void OnEnable()
    {
        UpdatePlayerLobbyUI();
    }


    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyMemberJoined -= OnMenuLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnMenuLobbyMemberLeave;
        SteamMatchmaking.OnLobbyGameCreated -= OnMenuLobbyGameCreated;
        SteamMatchmaking.OnLobbyMemberDisconnected -= OnMenuLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyEntered -= OnMenuLobbyEntered;
    }
    #endregion

    #region In Lobby
    private void OnMenuLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId id)
    {
        UpdatePlayerLobbyUI();
    }

    private void OnMenuLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        UpdatePlayerLobbyUI();
    }

    private void OnMenuLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        UpdatePlayerLobbyUI();
    }

    private void OnMenuLobbyMemberDisconnected(Lobby lobby, Friend friend)
    {
        UpdatePlayerLobbyUI();
    }

    private void OnMenuLobbyEntered(Lobby lobby)
    {
        UpdatePlayerLobbyUI();
    }


    private void UpdatePlayerLobbyUI()
    {
        if (!GameLobbyManager.instance.currentLobby.HasValue) return;
        if (lobbyName)
        {
            lobbyName.text = $"{GameLobbyManager.instance.currentLobby.Value.Owner.Name}'s Lobby";

        }
        if (lobbyName_join)
        {
            lobbyName_join.text = $"{GameLobbyManager.instance.currentLobby.Value.Owner.Name}'s Lobby";
        }
        int count = 0;
        foreach (Friend friend in GameLobbyManager.instance.currentLobby.Value.Members)
        {
            playerList[count].text = friend.Name;
            playerList_join[count].text = friend.Name;
            count++;
        }
        for (int i = count; i < playerList.Length; i++)
        {
            playerList[i].text = "Waiting for Player...";
            playerList_join[i].text = "Waiting for Player...";
        }
    }
    #endregion

    public async void SearchLobbies()
    {
        lobbies.Clear();
        filteredLobbies.Clear();
        foreach (LobbyInfoUI li in lobbyInfoList)
        {
            Destroy(li.gameObject);
        }
        lobbyInfoList.Clear();

        Lobby[] lobbyList = await lobbyQuery.RequestAsync();
        if (lobbyList != null)
        {
            lobbies.AddRange(await lobbyQuery.RequestAsync());
        }
        if (lobbies.Count == 0)
        {
            print("No Lobbies");
            return;
        }
        bool hasLobby = false;
        foreach (Lobby lobby in lobbies)
        {
            if (lobby.GetData(GameLobbyManager.GAMEKEYNAME) == GameLobbyManager.GAMENAME)
            {
                print($"{lobby.GetData(GameLobbyManager.LOBBYNAMEKEY)}");
                filteredLobbies.Add(lobby);
                hasLobby = true;
            }
        }
        if (!hasLobby)
        {
            print("No Lobbies");
            return;
        }
        int i = 0;
        foreach (Lobby lobby in filteredLobbies)
        {
            if (i >= maxLobbiesInPage) break;
            LobbyInfoUI lobbyInfoInstance = Instantiate(lobbyInfoPrefab, lobbyInfoContainer.transform);
            lobbyInfoInstance.GetHostNameText().text = lobby.GetData(GameLobbyManager.LOBBYNAMEKEY);
            lobbyInfoInstance.GetPlayerCountText().text = $"{lobby.MemberCount}/{lobby.MaxMembers}";
            lobbyInfoInstance.GetButton().onClick.AddListener(async () =>
            {
                RoomEnter result = await lobby.Join();
                if (result == RoomEnter.Success)
                {
                    lobbySearchObject.SetActive(false);
                    joinLobbyObject.SetActive(true);
                    GameLobbyManager.instance.ConnectToLobby(lobby);
                }
            });
            lobbyInfoList.Add(lobbyInfoInstance);
            i++;
        }

    }

}
