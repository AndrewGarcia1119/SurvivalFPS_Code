using UnityEngine;

namespace ShooterSurvival.Players
{
    public class FasterReload : Perk
    {
        float reloadSpeedDivider = 3f;

        public override void ActivatePerkForPlayer(Player p)
        {
            p.GetComponent<WeaponHandler>().personalReloadSpeedDivider.Value *= reloadSpeedDivider;
        }

        public override void RemovePerkForPlayer(Player p)
        {
            p.GetComponent<WeaponHandler>().personalReloadSpeedDivider.Value /= reloadSpeedDivider;
        }
    }
}