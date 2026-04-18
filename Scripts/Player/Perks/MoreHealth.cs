using Unity.Mathematics;
using UnityEngine;

namespace ShooterSurvival.Players
{
    public class MoreHealth : Perk
    {

        private float additionalHealth = 100;

        public override void ActivatePerkForPlayer(Player p)
        {
            p.AddTotalHealth(additionalHealth);
        }

        public override void RemovePerkForPlayer(Player p)
        {
            p.AddTotalHealth(-additionalHealth);
        }
    }
}
