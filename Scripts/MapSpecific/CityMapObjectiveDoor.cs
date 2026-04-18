using System;
using System.Collections;
using ShooterSurvival.GameSystems;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class CityMapObjectiveDoor : NetworkBehaviour, InteractiveObject
{
    [SerializeField]
    private byte wheelsNeeded = 4;
    [SerializeField]
    private string objectiveCompleteInteractionText;
    [SerializeField]
    private AudioSource carStartupAudioSource, rocketTakeOffSource;
    [SerializeField]
    string animStateName = "Open";
    [SerializeField]
    [Tooltip("Events that occur when the objective is complete (including interaction with ambulance). Gets called on server/host")]
    UnityEvent onObjectiveCompleteAndInteraction;
    [SerializeField]
    [Tooltip("Events that occur when the objective is complete (including interaction with ambulance). Gets called on servers/host and clients")]
    UnityEvent onObjectiveCompleteAndInteractionClient;
    [SerializeField]
    bool deleteOnInteraction = false;
    [SerializeField]
    float deleteTimer = 4f;

    private NetworkVariable<byte> wheelsCollected = new();
    private NetworkVariable<bool> hasSteeringWheel = new();
    private NetworkVariable<bool> isObjectiveComplete = new();

    private string interactionText;
    private bool successfulInteraction = false;

    Animator doorAnimator;

    //for server use only
    private Coroutine currentRoutine = null;


    public bool AbleToInteract(Interactor interactor)
    {
        return !successfulInteraction;
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            wheelsCollected.Value = 0;
            hasSteeringWheel.Value = false;
            isObjectiveComplete.Value = false;
        }
        doorAnimator = GetComponent<Animator>();
        UpdateInteractionText();
    }
    public string GetInteractionText()
    {
        return isObjectiveComplete.Value ? objectiveCompleteInteractionText : interactionText;
    }

    public InteractiveButton GetInteractiveButton()
    {
        return InteractiveButton.PRIMARY;
    }

    public void OnInteract(Interactor interactor, bool InteractedThisFrame)
    {
        if (!isObjectiveComplete.Value) return;
        if (InteractedThisFrame && !successfulInteraction)
        {
            CompleteInteractionServerRpc();
            CompleteInteractionClientRpc();
            interactor.RemoveAvailableInteractive();
        }
    }

    public void AddWheel()
    {
        AddItem(false); //NOTE: THIS SHOULD ONLY BE CALLED ON SERVER
    }

    public void AddSteeringWheel()
    {
        AddItem(true); //NOTE: THIS SHOULD ONLY BE CALLED ON SERVER
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<Interactor>(out Interactor interactor))
        {
            if (AbleToInteract(interactor))
            {
                UpdateInteractionText();
                interactor.AddAvailableInteractive(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Interactor>(out Interactor interactor))
        {
            UpdateInteractionText();
            interactor.RemoveAvailableInteractive();
        }
    }


    //NOTE: THIS SHOULD ONLY BE CALLED ON SERVER
    private void AddItem(bool isSteeringWheel)
    {
        if (isSteeringWheel)
        {
            hasSteeringWheel.Value = true;
        }
        else
        {
            wheelsCollected.Value++;
        }

        if (wheelsCollected.Value >= wheelsNeeded && hasSteeringWheel.Value)
        {
            isObjectiveComplete.Value = true;
        }
        else
        {
            UpdateInteractionText();
        }
    }

    //may need to become an RPC if this only runs on host when running network animator
    public void PlayCarStartup()
    {
        carStartupAudioSource.Play();
    }
    //may need to become an RPC if this only runs on host when running network animator
    public void PlayRocketTakeoff()
    {
        rocketTakeOffSource.Play();
    }

    private void UpdateInteractionText()
    {
        interactionText = $"Items Needed:\n Wheels ({wheelsCollected.Value}/{wheelsNeeded}),\nSteering Wheel({(hasSteeringWheel.Value ? 1 : 0)}/{1})";
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void CompleteInteractionServerRpc()
    {
        doorAnimator.Play(animStateName, 0, 0);
        if (deleteOnInteraction && currentRoutine == null)
        {
            currentRoutine = StartCoroutine(DestroyInSeconds(deleteTimer));
        }
        onObjectiveCompleteAndInteraction?.Invoke();
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void CompleteInteractionClientRpc()
    {
        successfulInteraction = true;
        onObjectiveCompleteAndInteractionClient?.Invoke();
    }


    private IEnumerator DestroyInSeconds(float timer)
    {
        yield return new WaitForSeconds(timer);
        NetworkObject.Despawn();
        Destroy(gameObject);
    }
    public void ResetInteractive()
    {
        return;
    }

}
