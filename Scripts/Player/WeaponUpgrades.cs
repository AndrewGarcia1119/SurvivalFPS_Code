using UnityEngine;

namespace ShooterSurvival.Players
{
	//all available weapon upgrades. Each weaponInfo should have an upgrade slot/slots. Each slot will have specific available upgrades.
	public enum WeaponUpgrades
	{
		FLAME_BULLET, //damage over time
		ICE_BULLET, //slow zombies down (stacks to some extent) -> may cancel out fire bullets
		CHAIN_BULLET, // chance of hitting another zombie
		ECHO_BULLET, // chance of hitting same zombie again with bullet. Possible options, immediate, delay for every echo (echo can occur multiple times but chance lowers on every success)
		INCREASED_RANGE, // increase range, if there is range based damage drop, this drop will occur less
		PENETRATE_BONUS, // either allows bullet penetration or increases it if already allowed
		INCREASED_AMMO, //increases ammo capacity in and out of mag
		INCREASED_DAMAGE, // increases weapon base damage
		NONE
	}
}