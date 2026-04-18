using Unity.Netcode;
using UnityEngine;
using ShooterSurvival.GameSystems;
using System.Collections.Generic;
using System.Threading;

namespace ShooterSurvival.AI
{
	public class EnemySpawnLocation : NetworkBehaviour
	{
		[SerializeField]
		float spawnTimer = 4f;
		[SerializeField]
		float spawnVariance = 2f;
		[SerializeField]
		[Tooltip("Must be prefab in network object list")]
		GameObject[] enemyPrefabs = null;
		[SerializeField]
		[Tooltip("Map Zones that can be entered to activate this location")]
		MapZone[] mapZones;
		[SerializeField]
		float playerDistThreshold = 20f;

		public bool spawnerIsActive = false;

		float currentCooldown;
		float currentTimer;
		GameManager gm;
		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Awake()
		{
			currentTimer = 0;
			ResetCooldown();
		}
		private void Start()
		{
			gm = FindAnyObjectByType<GameManager>();
		}

		// Update is called once per frame
		private void Update()
		{
			if (!IsServer) return;
			if (!CanSpawnEnemies())
			{
				currentTimer = 0;
				ResetCooldown();
				return;
			}
			currentTimer += Time.deltaTime;
			if (currentTimer >= currentCooldown)
			{
				if (gm.GetEnemyCount() < gm.GetMaxEnemies() && gm.TryDecrementEnemiesInRound())
				{
					int rand = Random.Range(0, enemyPrefabs.Length);
					var prefabInstance = Instantiate(enemyPrefabs[rand], transform.position, transform.rotation);
					var piNetworkObject = prefabInstance.GetComponent<NetworkObject>();
					piNetworkObject.Spawn(true);
					prefabInstance.transform.SetParent(gm.GetEnemyPoolTransform());
				}
				currentTimer = 0;
				ResetCooldown();
			}
		}

		private void ResetCooldown()
		{
			currentCooldown = spawnTimer + Random.Range(spawnTimer - spawnVariance, spawnTimer + spawnVariance);
		}

		private bool CanSpawnEnemies()
		{
			if (!spawnerIsActive) return false;
			foreach (MapZone mz in mapZones)
			{
				if (mz.playersInZone.Count > 0)
				{
					return true;
				}
			}
			foreach (var player in gm.currentPlayers)
			{
				if (Vector3.Distance(transform.position, player.transform.position) < playerDistThreshold)
				{
					return true;
				}
			}
			return false;
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere(transform.position, playerDistThreshold);
		}

		public void SetSpawnerIsActive(bool isActive)
		{
			spawnerIsActive = isActive;
		}
	}

}