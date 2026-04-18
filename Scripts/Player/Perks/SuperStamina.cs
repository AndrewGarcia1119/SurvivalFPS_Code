using UnityEngine;
using Unity.Netcode;
using ShooterSurvival.Util;

namespace ShooterSurvival.Players
{
	public class SuperStamina : Perk
	{
		float staminaBonus = 1.5f;
		float staminaRegenBonus = 1.5f;

		public override void ActivatePerkForPlayer(Player p)
		{
			p.staminaBonus.Value += staminaBonus;
			p.staminaRegenBonus.Value += staminaRegenBonus;
		}

		public override void RemovePerkForPlayer(Player p)
		{
			p.staminaBonus.Value -= staminaBonus;
			p.staminaRegenBonus.Value -= staminaRegenBonus;
		}

	}
}