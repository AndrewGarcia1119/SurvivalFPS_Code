using UnityEngine;
using Unity.Netcode;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    public bool connected;
    public bool inGame;
    public bool isHost;
    public ulong myClientId;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void HostCreated()
    {
        isHost = true;
        connected = true;
    }

    public void ConnectedAsClient()
    {
        isHost = false;
        connected = true;
    }

    public void Disconnected()
    {
        isHost = false;
        connected = false;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
