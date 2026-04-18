using UnityEngine;
using ShooterSurvival.Players;

namespace ShooterSurvival.GameSystems
{
    public class WeaponUpgrader : MonoBehaviour, InteractiveObject
    {
        [SerializeField]
        private int price = 7500;
        [SerializeField]
        private int priceAddition = 5000;

        private int currentPrice;


        private string interactionText;

        private void Awake()
        {
            currentPrice = price;
            interactionText = $"press E to upgrade your weapon ({price} points)";
        }

        public void OnInteract(Interactor interactor, bool InteractedThisFrame)
        {
            if (!InteractedThisFrame) return;
            Player p = interactor.GetComponent<Player>();
            if (p.CanAfford(currentPrice))
            {
                interactor.GetComponent<Player>().SubtractPointsRpc(currentPrice);
                interactor.GetComponent<WeaponHandler>().UpgradeWeapon();
                interactor.RemoveAvailableInteractive();
            }
        }

        public string GetInteractionText()
        {
            return interactionText;
        }

        public bool AbleToInteract(Interactor interactor)
        {
            //TODO: change this to depend on if the current equipped weapon is able to be upgraded
            return true;
        }

        public InteractiveButton GetInteractiveButton()
        {
            return InteractiveButton.PRIMARY;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent<Interactor>(out Interactor interactor))
            {
                if (interactor.IsOwner)
                {
                    currentPrice = price + (priceAddition * interactor.GetComponent<WeaponHandler>().ActiveWeaponLevel());
                    interactionText = $"press E to upgrade your weapon ({currentPrice} points)";
                }
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