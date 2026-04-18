using UnityEngine;
using UnityEngine.UI;
using ShooterSurvival.Players;
using ShooterSurvival.Util;

public class PlayerUI : MonoBehaviour
{
	[SerializeField]
	Slider healthSlider = null;
	[SerializeField]
	Slider staminaSlider = null;

	Player player;
	bool gotPlayer = false;
	// Update is called once per frame
	void Update()
	{
		if (!player && !gotPlayer) //gotPlayer boolean prevents regetting the player when it becomes null after disconnection
		{
			if (NetworkUtil.GetLocalPlayer())
			{
				player = NetworkUtil.GetLocalPlayer().GetComponent<Player>();
				gotPlayer = true;
			}
			else
			{
				return;
			}
		}
		if (healthSlider)
		{
			healthSlider.value = player.GetHealthPercentage();
		}
		if (staminaSlider)
		{
			staminaSlider.value = player.GetStaminaPercentage();
		}
	}
}
