using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using ShooterSurvival.Players;

namespace ShooterSurvival.GameSystems
{
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(WeaponHandler))]
    public class Interactor : NetworkBehaviour
    {
        [HideInInspector]
        public PlayerInput playerInput;
        [HideInInspector]
        public InteractiveObject availableInteractive = null;


        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                playerInput = new PlayerInput();
                playerInput.Player.Interact.Enable();
                playerInput.Player.Interact2.Enable();
            }
        }

        public void AddAvailableInteractive(InteractiveObject interactiveObject)
        {
            if (!IsOwner) return;
            availableInteractive = interactiveObject;
        }
        public void RemoveAvailableInteractive()
        {
            if (!IsOwner) return;
            if (availableInteractive != null)
            {
                availableInteractive.ResetInteractive();
            }
            availableInteractive = null;
        }
        void Update()
        {
            if (!IsOwner) return;
            if (availableInteractive == null) return;
            if (playerInput.Player.Interact.IsPressed() && availableInteractive.AbleToInteract(this) && availableInteractive.GetInteractiveButton() == InteractiveButton.PRIMARY)
            {
                availableInteractive.OnInteract(this, playerInput.Player.Interact.WasPressedThisFrame());
            }
            if (playerInput.Player.Interact2.IsPressed() && availableInteractive.AbleToInteract(this) && availableInteractive.GetInteractiveButton() == InteractiveButton.SECONDARY)
            {
                availableInteractive.OnInteract(this, playerInput.Player.Interact2.WasPressedThisFrame());
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                playerInput.Player.Interact.Disable();
                playerInput.Player.Interact2.Disable();
            }

        }

        public bool TryGetCurrentInteractiveText(out string text)
        {
            if (availableInteractive != null)
            {
                text = availableInteractive.GetInteractionText();
                return true;
            }
            else
            {
                text = null;
                return false;
            }
        }
    }

}