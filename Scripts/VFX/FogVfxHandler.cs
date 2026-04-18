using Unity.Netcode;
using UnityEngine;

public class FogVfxHandler : NetworkBehaviour
{
    [SerializeField]
    private ParticleSystem fog;
    [SerializeField]
    float startingTime = 18f;

    void Start()
    {
        fog.Simulate(startingTime);
        fog.Play();
    }
    public void TurnOffFog()
    {
        TurnOffFogRpc();
    }
    [Rpc(SendTo.Everyone)]
    private void TurnOffFogRpc()
    {
        var emission = fog.emission;
        emission.rateOverTime = 0.0f;
    }
}
