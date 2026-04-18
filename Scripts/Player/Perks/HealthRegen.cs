using UnityEngine;

namespace ShooterSurvival.Players
{
	public class HealthRegen : Perk
	{

		private float additionalHealthRegen = 4;

		public override void ActivatePerkForPlayer(Player p)
		{
			p.AddHealthRegen(additionalHealthRegen);
		}

		public override void RemovePerkForPlayer(Player p)
		{
			p.AddHealthRegen(-additionalHealthRegen);
		}
	}
}