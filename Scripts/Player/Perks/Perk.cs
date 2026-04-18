using UnityEngine;

namespace ShooterSurvival.Players
{
    public abstract class Perk
    {
        public abstract void ActivatePerkForPlayer(Player p);
        public abstract void RemovePerkForPlayer(Player p);
    }

}