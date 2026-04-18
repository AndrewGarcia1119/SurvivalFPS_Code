using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using ShooterSurvival.Players;
using System.Collections;
using UnityEngine.Events;

namespace ShooterSurvival.GameSystems
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(NetworkAnimator))]
    public class BuyableDoor : NetworkBehaviour, InteractiveObject
    {

        [SerializeField]
        int price = 1500;
        [SerializeField]
        string animStateName = "Open";
        [SerializeField]
        bool deleteOnOpen = false;
        [SerializeField]
        float deleteTimer = 4f;
        [SerializeField]
        [Tooltip("Events that occur when the door opens. Gets called on server/host")]
        UnityEvent onOpen;
        [SerializeField]
        [Tooltip("Events that occur when the door opens. Gets called on servers/host and clients")]
        UnityEvent onOpenClient;
        [SerializeField]
        bool useCustomInteractionText = false;

        [SerializeField]
        string customText = "";

        private string interactionText;

        Animator doorAnimator;
        NetworkVariable<bool> doorOpenable = new NetworkVariable<bool>();

        //for server use only
        private Coroutine currentRoutine = null;


        public bool AbleToInteract(Interactor interactor)
        {
            return doorOpenable.Value;
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
                interactor.GetComponent<Player>().SubtractPointsRpc(price);
                OpenDoorServerRpc();
                OpenDoorClientRpc();
                interactor.RemoveAvailableInteractive();
            }
        }

        private void Awake()
        {
            doorAnimator = GetComponent<Animator>();
            if (!useCustomInteractionText)
            {
                interactionText = $"Press E to open door ({price} points)";
            }
            else
            {
                interactionText = customText + $" ({price} points)";
            }
        }
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                doorOpenable.Value = true;
            }
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

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void OpenDoorServerRpc()
        {
            doorOpenable.Value = false;
            doorAnimator.Play(animStateName, 0, 0);
            if (deleteOnOpen && currentRoutine == null)
            {
                currentRoutine = StartCoroutine(DestroyInSeconds(deleteTimer));
            }
            onOpen?.Invoke();
        }
        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
        private void OpenDoorClientRpc()
        {
            onOpenClient?.Invoke();
        }

        private IEnumerator DestroyInSeconds(float timer)
        {
            yield return new WaitForSeconds(timer);
            NetworkObject.Despawn();
            Destroy(gameObject);
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