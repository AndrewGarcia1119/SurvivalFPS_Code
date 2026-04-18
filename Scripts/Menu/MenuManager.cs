using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    [Serializable]
    class LobbyCreateButton
    {
        public Button button;
        public int lobbySize;
    }

    [Serializable]
    class LobbyRemoveButton
    {
        public Button button;
    }

    [SerializeField]
    string currentSceneToLoad = "TesterScene";
    [SerializeField]
    [Tooltip("UI to disable when joining another lobby through invite")]
    private GameObject[] uiDisableList = null;
    [SerializeField]
    private GameObject joinLobbyUI = null;
    [SerializeField]
    private GameObject hostLobbyUI = null;
    [SerializeField]
    LobbyCreateButton[] lobbyCreateButtons = null;
    [SerializeField]
    LobbyRemoveButton[] lobbyRemoveButtons = null;




    GameLobbyManager glm;

    // private void OnSceneEvent(SceneEvent sceneEvent)
    // {
    //     Debug.Log($"Scene Event: {sceneEvent.SceneEventType} on {sceneEvent.ClientId}");
    // }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        GameLobbyManager.onJoinedLobby += GotoJoinedLobbyMenu;
        glm = FindAnyObjectByType<GameLobbyManager>();
        if (lobbyCreateButtons != null)
        {
            foreach (LobbyCreateButton lcb in lobbyCreateButtons)
            {
                lcb.button.onClick.AddListener(() =>
                {
                    glm.StartHost(lcb.lobbySize);
                });
            }
        }
        if (lobbyRemoveButtons != null)
        {
            foreach (LobbyRemoveButton lrb in lobbyRemoveButtons)
            {
                lrb.button.onClick.AddListener(glm.Disconnected);
            }
        }
        // if (glm.currentLobby != null)
        // {
        //     if (glm.currentLobby.Value.Owner.IsMe)
        //     {
        //         glm.currentLobby?.SetJoinable(glm.currentLobby.Value.MaxMembers > 1);
        //         GotoHostLobbyMenu();
        //     }
        //     else
        //     {
        //         GotoJoinedLobbyMenu();
        //     }
        // }
        if (glm.currentLobby != null)
        {
            glm.currentLobby?.Leave();
        }
    }

    void OnDisable()
    {
        GameLobbyManager.onJoinedLobby -= GotoJoinedLobbyMenu;
    }

    private void GotoJoinedLobbyMenu()
    {
        foreach (GameObject toDisable in uiDisableList)
        {
            toDisable.SetActive(false);
        }
        joinLobbyUI.SetActive(true);
    }
    private void GotoHostLobbyMenu()
    {
        foreach (GameObject toDisable in uiDisableList)
        {
            toDisable.SetActive(false);
        }
        hostLobbyUI.SetActive(true);
    }
    public void LoadMultiplayer()
    {
        // NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
        //NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Single);
        if (NetworkManager.Singleton.IsHost)
        {
            GameLobbyManager.instance.currentLobby.Value.SetJoinable(false);
            var status = NetworkManager.Singleton.SceneManager.LoadScene(currentSceneToLoad, LoadSceneMode.Single);

            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning("Failed to load Scene " +
                      $"with a {nameof(SceneEventProgressStatus)}: {status}");
            }
        }
    }

    public void SetSceneToLoad(string sceneName)
    {
        currentSceneToLoad = sceneName;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
