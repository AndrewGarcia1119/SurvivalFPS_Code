using System.Collections;
using Unity.Multiplayer.Playmode;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using ShooterSurvival.Players;
using ShooterSurvival.GameSystems;
using ShooterSurvival.Util;
using System;
using System.Data;

namespace ShooterSurvival.AI
{
	public class Enemy : NetworkBehaviour
	{
		[SerializeField]
		private float despawnTimer = 5f;
		[SerializeField]
		private float aiTargetUpdateInterval = 1f;
		[SerializeField]
		private Vector3 powerupSpawnOffset = Vector3.zero;
		[SerializeField]
		private string attackAnimName = "Attack";
		[SerializeField]
		private Melee melee;
		[SerializeField]
		[Tooltip("Layer of colliders when character dies")]
		private string deadLayerName = "DeadEnemy";
		[SerializeField]
		float turnSpeed = 5f;
		[SerializeField]
		[Tooltip("Determines how accurately the enemy must look at you before it stops turning")]
		float turnTolerance = 5f;


		//component references
		private NavMeshAgent agent;
		Animator enemyAnimator;


		//All available enemy actions
		private ChaseAction chaseAction;
		private AttackAction attackAction;

		//enemy state
		private NetworkVariable<float> currentHealth = new NetworkVariable<float>();
		private Player currentTarget;
		float aiUpdateTimer;
		private NetworkVariable<ulong> lastDamagedByClientId = new NetworkVariable<ulong>();
		private EnemyAction currentEnemyAction = null;
		private int deadLayerVal;

		//public state
		public float CurrentHealth { get { return currentHealth.Value; } }

		// used for calculations
		NavMeshPath path;
		//temporary for testing
		internal GameManager gm;

		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Awake()
		{
			SetUpActionState();
			gm = FindAnyObjectByType<GameManager>();
			agent = GetComponent<NavMeshAgent>();
			enemyAnimator = GetComponent<Animator>();
			aiUpdateTimer = aiTargetUpdateInterval;
			path = new NavMeshPath();
			deadLayerVal = LayerMask.NameToLayer(deadLayerName);
		}

		private void SetUpActionState()
		{
			chaseAction = new ChaseAction(this);
			attackAction = new AttackAction(this);
		}
		public override void OnNetworkSpawn()
		{
			if (IsServer)
			{
				ResetEnemy(true);

			}
			currentHealth.OnValueChanged += OnHealthChanged;


		}
		private void Start()
		{
			ResetEnemy(false);
			if (IsServer)
			{
				UpdateCurrentTarget();
				SetNewAction(chaseAction);
			}
			//melee = GetComponentInChildren<Melee>();
		}
		// Update is called once per frame
		void Update()
		{
			if (!IsServer) return;
			if (currentHealth.Value <= 0) return;
			if (!agent.enabled) return;
			aiUpdateTimer += Time.deltaTime;
			if (currentEnemyAction == null) return;
			currentEnemyAction.OnUpdateAction();
			// if (aiUpdateTimer >= aiTargetUpdateInterval)
			// {
			// 	aiUpdateTimer = 0;
			// 	UpdateCurrentTarget();
			// }
			// if (!currentTarget)
			// {
			// 	enemyAnimator.SetBool("Moving", false);
			// 	return;
			// }
			// agent.SetDestination(currentTarget.transform.position);
			// enemyAnimator.SetBool("Moving", agent.remainingDistance >= agent.stoppingDistance);

		}

		void ResetEnemy(bool includeHealth)
		{
			if (includeHealth)
				currentHealth.Value = gm.CurrentEnemyHealth;
			foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
			{
				rb.isKinematic = true;
			}

		}

		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
		internal void TakeDamageServerRpc(float damage, ulong sourceClientId)
		{
			if (currentHealth.Value > 0)
			{
				lastDamagedByClientId.Value = sourceClientId;
				currentHealth.Value -= damage * gm.GetDamageMultiplier();
			}

		}

		private void OnHealthChanged(float previous, float current)
		{
			if (current <= 0 && previous > 0)
			{
				GetComponent<NetworkObject>().TryRemoveParent();
				enemyAnimator.enabled = false;
				foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
				{
					rb.isKinematic = false;
					rb.gameObject.layer = deadLayerVal;
				}
				agent.enabled = false;
				if (IsServer)
				{
					NetworkObject no = NetworkUtil.GetPlayerById(lastDamagedByClientId.Value);
					if (no)
					{
						no.GetComponent<Player>().AddPoints(gm.GetKillPoints(false));
					}
					gm.TrySpawnPowerupOnChance(transform.position + powerupSpawnOffset, transform.rotation);
					Destroy(gameObject, despawnTimer);
				}
			}
			print($"health left{current}");
			print($"damage done {previous - current}");
		}

		void UpdateCurrentTarget()
		{
			foreach (Player p in gm.currentPlayers)
			{
				if (p.isActiveAndEnabled && p.Targetable && p != currentTarget)
				{
					if (currentTarget == null || !currentTarget.Targetable || !currentTarget.isActiveAndEnabled)
					{
						currentTarget = p;
						continue;
					}
					if (p == null)
					{
						return;
					}
					if (Vector3.SqrMagnitude(currentTarget.transform.position - transform.position) < Vector3.SqrMagnitude(p.transform.position - transform.position) || !NavMesh.CalculatePath(transform.position, p.transform.position, NavMesh.AllAreas, path)) continue;
					float pathDist = 0;
					for (int i = 1; i < path.corners.Length; i++)
					{
						pathDist += Mathf.Abs(Vector3.Distance(path.corners[i], path.corners[i - 1]));
					}
					if (agent.remainingDistance >= pathDist)
					{
						currentTarget = p;
					}
				}
			}
		}

		private void SetNewAction(EnemyAction action)
		{
			if (currentEnemyAction == action) return;
			if (currentEnemyAction != null)
				currentEnemyAction.OnEndAction();
			currentEnemyAction = action;
			action.OnStartAction();
		}

		public void SetDefaultAction()
		{
			if (currentEnemyAction == chaseAction) return;
			if (currentEnemyAction != null)
				currentEnemyAction.OnEndAction();
			currentEnemyAction = chaseAction;
			chaseAction.OnStartAction();
		}

		public void ActivateMeleeHitbox()
		{
			if (IsServer && melee)
			{
				melee.gameObject.SetActive(true);
			}
		}

		public void DeactivateMeleeHitbox()
		{
			if (IsServer && melee)
			{
				melee.gameObject.SetActive(false);
			}
		}


		private abstract class EnemyAction
		{
			protected Enemy outerClass;

			public EnemyAction(Enemy oc)
			{
				outerClass = oc;
			}
			public abstract void OnStartAction();
			public abstract void OnUpdateAction();
			public abstract void OnEndAction();
		}

		private class ChaseAction : EnemyAction
		{
			public ChaseAction(Enemy oc) : base(oc) { }

			public override void OnStartAction()
			{
				outerClass.agent.isStopped = false;
			}

			public override void OnUpdateAction()
			{
				if (outerClass.aiUpdateTimer >= outerClass.aiTargetUpdateInterval)
				{
					outerClass.aiUpdateTimer = 0;
					outerClass.UpdateCurrentTarget();
				}
				if (!outerClass.currentTarget)
				{
					outerClass.enemyAnimator.SetBool("Moving", false);
					return;
				}
				outerClass.agent.SetDestination(outerClass.currentTarget.transform.position);
				if (outerClass.agent.remainingDistance >= outerClass.agent.stoppingDistance)
				{
					outerClass.enemyAnimator.SetBool("Moving", true);
				}
				else
				{
					outerClass.enemyAnimator.SetBool("Moving", false);
					if (!outerClass.agent.pathPending)
						outerClass.SetNewAction(outerClass.attackAction);
				}
			}

			public override void OnEndAction()
			{
				outerClass.agent.isStopped = true;
			}
		}

		private class AttackAction : EnemyAction
		{
			public AttackAction(Enemy oc) : base(oc) { }

			public override void OnStartAction()
			{
				outerClass.enemyAnimator.Play(outerClass.attackAnimName, 0, 0);
				if (outerClass.agent)
				{
					outerClass.agent.updateRotation = false;
					outerClass.agent.ResetPath();
				}
			}

			public override void OnUpdateAction()
			{
				if (NeedsToTurnToTarget(out Quaternion lookOnLook))
				{
					outerClass.transform.rotation = Quaternion.RotateTowards(outerClass.transform.rotation, lookOnLook, Time.deltaTime * outerClass.turnSpeed);
				}
			}

			public override void OnEndAction()
			{
				outerClass.enemyAnimator.Play("Idle", 0, 0);
				if (outerClass.agent)
				{
					outerClass.agent.updateRotation = true;
				}
			}

			private bool NeedsToTurnToTarget(out Quaternion lookOnLook)
			{
				if (outerClass.currentTarget == null || !outerClass.currentTarget.Targetable || !outerClass.currentTarget.isActiveAndEnabled)
				{
					lookOnLook = new Quaternion();
					return false;
				}
				Vector3 direction = outerClass.currentTarget.transform.position
					- outerClass.transform.position;

				direction.y = 0;

				lookOnLook = Quaternion.LookRotation(direction);
				return Quaternion.Angle(outerClass.transform.rotation, lookOnLook) > outerClass.turnTolerance;
			}

		}

	}

}