using UnityEngine;
using ShooterSurvival.Players;

namespace ShooterSurvival.GameSystems
{
    public class BuyableWeapon : MonoBehaviour, InteractiveObject
    {
        [SerializeField]
        private WeaponInfo weaponInfo;
        [SerializeField]
        private int price = 1200;


        private string interactionText;

        private void Awake()
        {
            interactionText = $"press E to buy {weaponInfo.GetWeaponName()} ({price} points)";
        }

        public void OnInteract(Interactor interactor, bool InteractedThisFrame)
        {
            if (!InteractedThisFrame) return;
            Player p = interactor.GetComponent<Player>();
            if (p.CanAfford(price))
            {
                interactor.GetComponent<Player>().SubtractPointsRpc(price);
                interactor.GetComponent<WeaponHandler>().AddWeapon(weaponInfo);
                interactor.RemoveAvailableInteractive();
            }
        }

        public string GetInteractionText()
        {
            return interactionText;
        }

        public bool AbleToInteract(Interactor interactor)
        {
            return !interactor.GetComponent<WeaponHandler>().HasWeapon(weaponInfo);
        }

        public InteractiveButton GetInteractiveButton()
        {
            return InteractiveButton.PRIMARY;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Interactor>(out Interactor interactor))
            {
                if (AbleToInteract(interactor))
                {
                    interactor.AddAvailableInteractive(this);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<Interactor>(out Interactor interactor))
            {
                interactor.RemoveAvailableInteractive();
            }
        }
        public void ResetInteractive()
        {
            return;
        }
    }
}