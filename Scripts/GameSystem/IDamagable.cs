using ShooterSurvival.Players;
using UnityEngine;

namespace ShooterSurvival.GameSystems
{
	public interface IDamagable
	{
		void OnHit(float damage, ulong sourceClientId, WeaponUpgrades[] activeEffects = null, bool ignoreBodyPartMultipliers = false);
	}

}