using TMPro;
using UnityEngine;
using Unity.Netcode;
using ShooterSurvival.GameSystems;
using ShooterSurvival.Players;
using ShooterSurvival.Util;

public class GameManagerUI : NetworkBehaviour
{
    [SerializeField]
    TMP_Text currentRoundText, currentEnemyHealthText, remainingEnemiesText, tripleDamageTimerText;
    [SerializeField]
    TMP_Text[] playerPointsTextboxes;

    GameManager gm;
    // Update is called once per frame

    public override void OnNetworkSpawn()
    {
        gm = FindAnyObjectByType<GameManager>();
        gm.onPlayerCountUpdated += SetPlayerPointsText;
        SetPlayerPointsText();
        if (!IsServer)
        {
            currentEnemyHealthText.gameObject.SetActive(false);
            remainingEnemiesText.gameObject.SetActive(false);
            tripleDamageTimerText.gameObject.SetActive(false);
        }
    }
    public override void OnNetworkDespawn()
    {
        gm.onPlayerCountUpdated -= SetPlayerPointsText;
        base.OnNetworkDespawn();
    }
    void Update()
    {
        if (!gm) return;
        if (IsServer)
        {
            if (currentEnemyHealthText)
            {
                currentEnemyHealthText.text = $"Enemy Health: {gm.CurrentEnemyHealth}";
            }
            if (remainingEnemiesText)
            {
                remainingEnemiesText.text = $"Remaining Enemies To Spawn: {gm.RemainingEnemiesInRound}";
            }
            if (tripleDamageTimerText)
            {
                tripleDamageTimerText.text = $"triple damage timer: {gm.TripleDmgTimer}";
            }
        }
        if (currentRoundText)
        {
            currentRoundText.text = $"Round {gm.GetCurrentRound()}";
        }
        SetPlayerPointsText();
        for (int i = 0; i < gm.currentPlayers.Count; i++)
        {
            if (gm.currentPlayers[i])
            {
                if (NetworkUtil.GetLocalPlayer() && gm.currentPlayers[i] == NetworkUtil.GetLocalPlayer().GetComponent<Player>())
                {
                    playerPointsTextboxes[i].fontStyle = FontStyles.Bold;
                }
                playerPointsTextboxes[i].text = $"{gm.currentPlayers[i].CurrentPoints}";
            }
        }

    }

    private void SetPlayerPointsText()
    {
        for (int i = 0; i < playerPointsTextboxes.Length; i++)
        {
            playerPointsTextboxes[i].gameObject.SetActive(i < gm.currentPlayers.Count);
        }
    }
}
