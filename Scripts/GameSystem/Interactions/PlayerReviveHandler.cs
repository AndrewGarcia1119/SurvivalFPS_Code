using ShooterSurvival.GameSystems;
using UnityEngine;
using ShooterSurvival.Players;
using System;
namespace ShooterSurvival.GameSystems
{
    public class PlayerReviveHandler : MonoBehaviour, InteractiveObject
    {
        private string interactionText;

        //game manager handles how long it takes to revive a player.
        GameManager gm;
        Player player;
        float revivingTimer = 0f;
        bool ableToInteract = true;

        public float RevivingTimer { get { return revivingTimer; } }
        public static event Action<PlayerReviveHandler> onBeginRevive;

        private void Awake()
        {
            interactionText = $"hold F to revive";
        }
        private void OnEnable()
        {
            ableToInteract = true;
        }

        void Start()
        {
            player = GetComponentInParent<Player>();
            gm = FindAnyObjectByType<GameManager>();
        }
        public bool AbleToInteract(Interactor interactor)
        {
            return ableToInteract;
        }

        public string GetInteractionText()
        {
            return interactionText;
        }

        public InteractiveButton GetInteractiveButton()
        {
            return InteractiveButton.SECONDARY;
        }

        public void OnInteract(Interactor interactor, bool InteractedThisFrame)
        {
            if (InteractedThisFrame)
            {
                revivingTimer = 0f;
            }
            if (revivingTimer == 0f)
            {
                onBeginRevive?.Invoke(this);
            }
            revivingTimer += Time.deltaTime;
            if (revivingTimer >= gm.reviveTime)
            {
                revivingTimer = 0f;
                ableToInteract = false;
                interactor.RemoveAvailableInteractive();
                player.RevivePlayer(); //MUST BE LAST as reviving player disables this object
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Interactor>(out Interactor interactor))
            {
                if (AbleToInteract(interactor) && interactor.GetComponent<Player>() != player)
                {
                    interactor.AddAvailableInteractive(this);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<Interactor>(out Interactor interactor) && interactor.GetComponent<Player>() != player)
            {
                interactor.RemoveAvailableInteractive();
            }
        }
        public void ResetInteractive()
        {
            revivingTimer = 0f;
            return;
        }
    }
}