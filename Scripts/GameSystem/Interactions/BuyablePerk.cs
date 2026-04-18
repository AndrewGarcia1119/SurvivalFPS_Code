using ShooterSurvival.Players;
using Unity.Netcode;
using UnityEngine;

namespace ShooterSurvival.GameSystems
{
    [RequireComponent(typeof(Collider))]
    public class BuyablePerk : MonoBehaviour, InteractiveObject
    {
        [SerializeField]
        PerkType perkType;
        [SerializeField]
        string perkName = "perk";
        [SerializeField]
        int price = 3000;

        GameManager gm;
        string interactionText;

        public bool AbleToInteract(Interactor interactor)
        {
            return !interactor.GetComponent<Player>().HasPerk(perkType);
        }

        public string GetInteractionText()
        {
            return interactionText;
        }

        public void OnInteract(Interactor interactor, bool InteractedThisFrame)
        {
            if (!InteractedThisFrame) return;
            Player p = interactor.GetComponent<Player>();
            if (p.CanAfford(price))
            {
                p.SubtractPointsRpc(price);
                gm.AddPerkServerRpc(NetworkManager.Singleton.LocalClientId, perkType);
                interactor.RemoveAvailableInteractive();
            }
        }

        private void Start()
        {
            gm = FindAnyObjectByType<GameManager>();
            interactionText = $"Press E to buy {perkName} ({price} points)";
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

        public InteractiveButton GetInteractiveButton()
        {
            return InteractiveButton.PRIMARY;
        }
        public void ResetInteractive()
        {
            return;
        }
    }
}