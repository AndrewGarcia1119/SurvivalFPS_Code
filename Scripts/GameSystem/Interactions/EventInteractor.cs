using System.Collections;
using ShooterSurvival.GameSystems;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class EventInteractor : NetworkBehaviour, InteractiveObject
{
    [SerializeField]
    [Tooltip("Events that occur when object is interacted with. Gets called on server/host")]
    UnityEvent onInteract;
    [SerializeField]
    [Tooltip("Events that occur when the object is interacted with. Gets called on servers/host and clients")]
    UnityEvent onInteractClient;
    [SerializeField]
    bool deleteOnInteraction = false;
    [SerializeField]
    float deleteTimer = 4f;

    [SerializeField]
    string customText = "";
    [SerializeField]
    bool useCustomInteractionText = false;
    [SerializeField]
    bool interactOnlyOnce = true;

    bool hasInteracted = false;


    private string interactionText;

    private Coroutine currentRoutine = null;

    // private void Awake()
    // {
    //     if (!useCustomInteractionText)
    //     {
    //         interactionText = $"Press E to interact";
    //     }
    //     else
    //     {
    //         interactionText = customText;
    //     }
    // }

    public override void OnNetworkSpawn()
    {
        if (!useCustomInteractionText)
        {
            interactionText = $"Press E to interact";
        }
        else
        {
            interactionText = customText;
        }
    }
    public bool AbleToInteract(Interactor interactor)
    {
        if (!hasInteracted)
        {
            return true;
        }
        else
        {
            return !interactOnlyOnce;
        }
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    public InteractiveButton GetInteractiveButton()
    {
        return InteractiveButton.PRIMARY;
    }

    public void OnInteract(Interactor interactor, bool InteractedThisFrame)
    {
        if (!InteractedThisFrame) return;
        hasInteracted = true;
        CallEventServerRpc();
        CallEventClientRpc();
        DespawnServerRpc();
        interactor.RemoveAvailableInteractive();
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
    private void CallEventServerRpc()
    {
        onInteract?.Invoke();
    }
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void CallEventClientRpc()
    {
        onInteractClient?.Invoke();
    }

    private IEnumerator DestroyInSeconds(float timer)
    {
        yield return new WaitForSeconds(timer);
        NetworkObject.Despawn(true);
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DespawnServerRpc()
    {
        if (deleteOnInteraction && currentRoutine == null)
        {
            if (deleteTimer > 0)
            {
                currentRoutine = StartCoroutine(DestroyInSeconds(deleteTimer));
            }
            else
            {
                NetworkObject.Despawn(true);
            }
        }
    }
    public void ResetInteractive()
    {
        return;
    }


}
