using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInfoUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text hostNameText;
    [SerializeField]
    private TMP_Text playerCountText;
    [SerializeField]
    private Button button;

    public TMP_Text GetHostNameText()
    {
        return hostNameText;
    }
    public TMP_Text GetPlayerCountText()
    {
        return playerCountText;
    }

    public Button GetButton()
    {
        return button;
    }
}
