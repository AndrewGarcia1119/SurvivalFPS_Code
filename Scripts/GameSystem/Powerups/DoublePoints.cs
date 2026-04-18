using UnityEngine;
using Unity.Netcode;

namespace ShooterSurvival.GameSystems
{
    [RequireComponent(typeof(Collider))]
    public class DoublePoints : NetworkBehaviour, PowerUp
    {
        GameManager gm;

        public void ActivatePowerUp()
        {
            gm.StartPowerUp(this);
        }

        public bool IsPersonal()
        {
            return false;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                gm = FindAnyObjectByType<GameManager>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            if (other.CompareTag("Player"))
            {
                ActivatePowerUp();
                //GetComponent<NetworkObject>().Despawn();
                Destroy(gameObject);
            }
        }
    }
}
