using UnityEngine;

namespace ShooterSurvival.Players
{
    public class ExtraGun : Perk
    {
        int numGunSlotsToAdd = 1;

        public override void ActivatePerkForPlayer(Player p)
        {
            p.GetComponent<WeaponHandler>().currentMaxWeapons.Value += numGunSlotsToAdd;
        }

        public override void RemovePerkForPlayer(Player p)
        {
            p.GetComponent<WeaponHandler>().currentMaxWeapons.Value -= numGunSlotsToAdd;
        }
    }
}