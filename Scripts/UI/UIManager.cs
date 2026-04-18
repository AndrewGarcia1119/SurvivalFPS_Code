using ShooterSurvival.GameSystems;
using ShooterSurvival.Players;
using ShooterSurvival.Util;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    GameObject pauseMenuUI;
    [SerializeField]
    GameObject reviveUI;
    [SerializeField]
    Slider reviveBar;

    PauseManager pm;
    PlayerReviveHandler currentRH;
    GameManager gm;
    Interactor localPlayerInteractor;
    bool gotPlayer;

    void Start()
    {
        pm = FindAnyObjectByType<PauseManager>();
        gm = FindAnyObjectByType<GameManager>();
        if (!pm) return;
        pm.onLocalPlayerPause += pauseMenuUI.SetActive;
        PlayerReviveHandler.onBeginRevive += HandleOnBeginRevive;
    }

    void Update()
    {
        if (!localPlayerInteractor && !gotPlayer) //gotPlayer boolean prevents regetting the player when it becomes null after disconnection
        {
            if (NetworkUtil.GetLocalPlayer())
            {
                Player player = NetworkUtil.GetLocalPlayer().GetComponent<Player>();
                localPlayerInteractor = player.GetComponent<Interactor>();
                gotPlayer = true;
            }
            else
            {
                return;
            }
        }
        if (!gm) return;

        if (!localPlayerInteractor.playerInput.Player.Interact2.IsPressed() || localPlayerInteractor.availableInteractive == null || localPlayerInteractor.availableInteractive is not PlayerReviveHandler || (localPlayerInteractor.availableInteractive as PlayerReviveHandler) != currentRH)
        {
            currentRH = null;
        }
        if (!currentRH)
        {
            reviveBar.value = 0f;
            reviveUI.SetActive(false);
            return;
        }
        else
        {
            reviveBar.value = Mathf.Min(1, currentRH.RevivingTimer / gm.reviveTime);
        }
    }

    void OnDisable()
    {
        pm.onLocalPlayerPause -= pauseMenuUI.SetActive;
        PlayerReviveHandler.onBeginRevive -= HandleOnBeginRevive;
    }

    void HandleOnBeginRevive(PlayerReviveHandler rh)
    {
        reviveUI.SetActive(true);
        currentRH = rh;
    }
}
