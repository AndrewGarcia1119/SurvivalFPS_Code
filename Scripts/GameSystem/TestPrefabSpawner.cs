using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class TestPrefabSpawner : NetworkBehaviour
{
    [SerializeField]
    float spawnTimer = 2f;
    [SerializeField]
    NetworkObject networkPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnPrefab());
    }

    private IEnumerator SpawnPrefab()
    {
        yield return new WaitForSeconds(spawnTimer);
        if (networkPrefab && IsServer)
        {
            var p = Instantiate(networkPrefab, transform.position, transform.rotation) as NetworkObject;
            p.Spawn(true);
        }
    }
}
