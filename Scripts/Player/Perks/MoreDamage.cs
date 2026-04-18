using UnityEngine;
namespace ShooterSurvival.Players
{
    public class MoreDamage : Perk
    {
        private float damageMultiplier = 1.5f;


        public override void ActivatePerkForPlayer(Player p)
        {
            WeaponHandler wh = p.GetComponent<WeaponHandler>();
            wh.personalDamageMultiplier.Value = Mathf.Max(wh.personalDamageMultiplier.Value * damageMultiplier, damageMultiplier);
        }

        public override void RemovePerkForPlayer(Player p)
        {
            WeaponHandler wh = p.GetComponent<WeaponHandler>();
            wh.personalDamageMultiplier.Value = Mathf.Max(wh.personalDamageMultiplier.Value / damageMultiplier, 1);
        }
    }
}