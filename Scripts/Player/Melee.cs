using ShooterSurvival.GameSystems;
using Unity.Netcode;
using UnityEngine;

namespace ShooterSurvival.Players
{
    [RequireComponent(typeof(Collider))]
    public class Melee : MonoBehaviour
    {
        [SerializeField]
        float damage = 110f;

        [SerializeField]
        bool isPlayer = false;

        bool didHit = false;

        GameManager gm;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void OnEnable()
        {
            if (!gm)
            {
                gm = FindAnyObjectByType<GameManager>();
            }
            didHit = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (didHit) return;
            IDamagable damagable = other.GetComponentInParent<IDamagable>();
            if (damagable != null)
            {
                Player otherPlayer = other.GetComponentInParent<Player>();
                if (isPlayer && !gm.isFriendlyFireAllowed && otherPlayer) return;
                if (!isPlayer && !otherPlayer) return;
                damagable.OnHit(damage, NetworkManager.Singleton.LocalClientId, null, true);
                didHit = true;
            }
        }

        void OnDisable()
        {
            didHit = false;
        }
    }

}