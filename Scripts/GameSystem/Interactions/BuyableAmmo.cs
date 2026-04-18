using UnityEngine;
using ShooterSurvival.Players;

namespace ShooterSurvival.GameSystems
{
    public class BuyableAmmo : MonoBehaviour, InteractiveObject
    {
        private int currentPrice = -1;
        private string interactionText;
        public void OnInteract(Interactor interactor, bool InteractedThisFrame)
        {
            if (!InteractedThisFrame) return;
            Player p = interactor.GetComponent<Player>();
            if (p.CanAfford(currentPrice))
            {
                interactor.GetComponent<Player>().SubtractPointsRpc(currentPrice);
                interactor.GetComponent<WeaponHandler>().ResetAmmoRpc(currentWeaponOnly: true);
                interactor.RemoveAvailableInteractive();
            }
        }

        public string GetInteractionText()
        {
            if (currentPrice > 0)
                return interactionText;
            else
                return "";
        }

        public bool AbleToInteract(Interactor interactor)
        {
            return !interactor.GetComponent<WeaponHandler>().HasFullAmmo();
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
                    currentPrice = interactor.GetComponent<WeaponHandler>().GetCurrentWeaponAmmoRefillCost();
                    interactionText = $"press E to refill your ammo ({currentPrice} points, equipped weapon only)";
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