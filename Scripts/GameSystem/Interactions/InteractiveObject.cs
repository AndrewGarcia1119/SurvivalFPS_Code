using UnityEngine;
using UnityEngine.InputSystem;

namespace ShooterSurvival.GameSystems
{
    public interface InteractiveObject
    {
        public void OnInteract(Interactor interactor, bool InteractedThisFrame);
        public string GetInteractionText();
        public bool AbleToInteract(Interactor interactor);
        public InteractiveButton GetInteractiveButton();
        public void ResetInteractive(); //use for resetting objects when player is interacting and leaves mid-interaction
    }

}