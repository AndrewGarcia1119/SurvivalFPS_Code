using System.Collections.Generic;
using ShooterSurvival.Players;
using UnityEngine;

namespace ShooterSurvival.GameSystems
{
    [RequireComponent(typeof(Collider))]
    public class MapZone : MonoBehaviour
    {
        //[HideInInspector]
        [Tooltip("Should be left alone, only shown in inspector for debugging")]
        public List<Player> playersInZone = new();

        GameManager gm;

        void Start()
        {
            gm = FindAnyObjectByType<GameManager>();
            gm.onPlayerCountUpdated += RemoveDisconnectedPlayers;
            foreach (Collider c in GetComponents<Collider>())
            {
                c.isTrigger = true;
            }
        }

        void OnDisable()
        {
            gm.onPlayerCountUpdated -= RemoveDisconnectedPlayers;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Player>(out Player player))
            {
                playersInZone.Add(player);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<Player>(out Player player))
            {
                playersInZone.Remove(player);
            }
        }

        void RemoveDisconnectedPlayers()
        {
            for (int i = 0; i < playersInZone.Count; i++)
            {
                if (!gm.currentPlayers.Contains(playersInZone[i]))
                {
                    playersInZone.RemoveAt(i);
                    i--;
                }
            }
        }
    }

}