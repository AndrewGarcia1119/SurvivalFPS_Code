using UnityEngine;

namespace ShooterSurvival.Players
{
    public class FasterShooting : Perk
    {
        float shootSpeedMultiplier = 0.8f;

        public override void ActivatePerkForPlayer(Player p)
        {
            WeaponHandler wh = p.GetComponent<WeaponHandler>();
            wh.personalShootSpeedMultiplier.Value = Mathf.Min(wh.personalShootSpeedMultiplier.Value * shootSpeedMultiplier, shootSpeedMultiplier);
        }

        public override void RemovePerkForPlayer(Player p)
        {
            WeaponHandler wh = p.GetComponent<WeaponHandler>();
            wh.personalShootSpeedMultiplier.Value = Mathf.Min(wh.personalShootSpeedMultiplier.Value / shootSpeedMultiplier, 1);
        }
    }
}