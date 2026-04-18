using UnityEngine;

public class MenuParticleSystemController : MonoBehaviour
{
    [SerializeField]
    ParticleSystem ps = null;
    [SerializeField]
    float startingTime = 18f;
    void Start()
    {
        ps.Simulate(startingTime);
        ps.Play();
    }

}
