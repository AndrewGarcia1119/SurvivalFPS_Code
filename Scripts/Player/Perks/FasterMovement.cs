using UnityEngine;

namespace ShooterSurvival.Players
{
    public class FasterMovement : Perk
    {
        float moveSpeedMultiplier = 1.25f;

        public override void ActivatePerkForPlayer(Player p)
        {
            p.speedMultiplier.Value *= moveSpeedMultiplier;
        }

        public override void RemovePerkForPlayer(Player p)
        {
            p.speedMultiplier.Value /= moveSpeedMultiplier;
        }
    }
}