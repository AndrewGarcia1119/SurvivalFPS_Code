using System.Collections.Generic;
using ShooterSurvival.GameSystems;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.AI;


namespace ShooterSurvival.AI
{
    [RequireComponent(typeof(Enemy))]
    public class EnemyEffectHandler : NetworkBehaviour, IEffectHandler
    {
        [System.Serializable]
        public class AppliedEffectWrapper
        {
            public AppliedEffect effect;

            public AppliedEffectWrapper(AppliedEffect effect)
            {
                this.effect = effect;
            }
        }

        [SerializeField]
        [Tooltip("Determines the enemy to apply the effect on")]
        private Enemy enemy = null;

        [SerializeField]
        [Range(0, 1f)]
        private float frostChance = 0.1f;
        [SerializeField]
        [Range(0, 1f)]
        private float burnChance = 0.1f;

        [SerializeField]
        private ParticleSystem burnVfx;
        [SerializeField]
        private ParticleSystem frostVfx;

        [SerializeField]
        private float frostDebuffPerStack = 0.25f;

        [SerializeField] // temporary for debug
        List<AppliedEffectWrapper> activeEffectWrappers = new List<AppliedEffectWrapper>();

        Dictionary<AppliedEffectWrapper, Coroutine> effectCoroutines = new Dictionary<AppliedEffectWrapper, Coroutine>();


        private NavMeshAgent agent;
        Animator enemyAnimator;


        void Awake()
        {
            if (!enemy)
            {
                enemy = GetComponent<Enemy>();
            }
            agent = GetComponent<NavMeshAgent>();
            enemyAnimator = GetComponent<Animator>();
        }


        void Update()
        {
            for (int i = activeEffectWrappers.Count - 1; i >= 0; i--)
            {
                if (!effectCoroutines.ContainsKey(activeEffectWrappers[i]) || effectCoroutines[activeEffectWrappers[i]] == null)
                {
                    effectCoroutines.Add(activeEffectWrappers[i], StartCoroutine(DoEffect(activeEffectWrappers[i])));
                }
                //TODO: Add functionality for instantaneous effects.
            }
        }

        private IEnumerator DoEffect(AppliedEffectWrapper effectWrapper)
        {
            ToggleEffectVfxClientRpc(effectWrapper.effect.type, true); //This RPC only invokes from server, but this coroutine should only be running serverside.
            if (effectWrapper.effect.type == Effect.BURN)
            {
                float timer = 0f;
                while (effectWrapper.effect.duration > 0)
                {
                    effectWrapper.effect.duration -= Time.deltaTime;
                    timer += Time.deltaTime;
                    if (timer > 1)
                    {
                        timer = 0f;
                        enemy.TakeDamageServerRpc(50, effectWrapper.effect.sourceId);
                    }
                    yield return null;
                }
            }
            else if (effectWrapper.effect.type == Effect.FROST)
            {
                float agentSpeed = agent.speed;
                float animatorSpeed = enemyAnimator.speed;
                while (effectWrapper.effect.duration > 0)
                {
                    effectWrapper.effect.duration -= Time.deltaTime;
                    agent.speed = agentSpeed * (1f - (frostDebuffPerStack * effectWrapper.effect.currentStacks));
                    enemyAnimator.speed = animatorSpeed * (1f - (frostDebuffPerStack * effectWrapper.effect.currentStacks));
                    yield return null;
                }
                agent.speed = agentSpeed;
                enemyAnimator.speed = animatorSpeed;
            }
            if (effectCoroutines.ContainsKey(effectWrapper))
            {
                effectCoroutines[effectWrapper] = null;
            }
            activeEffectWrappers.Remove(effectWrapper);
            ToggleEffectVfxClientRpc(effectWrapper.effect.type, false); //This RPC only invokes from server, but this coroutine should only be running serverside.
        }

        public void ApplyEffect(AppliedEffect[] appliedEffects)
        {
            if (appliedEffects == null) return;
            ApplyEffectServerRpc(appliedEffects);
        }

        [Rpc(SendTo.Server)]
        private void ApplyEffectServerRpc(AppliedEffect[] appliedEffects)
        {
            foreach (AppliedEffect effect in appliedEffects)
            {
                if (effect.type == Effect.NONE) continue;
                if (effect.type == Effect.FROST)
                {
                    if (Random.Range(0f, 1f) > frostChance) continue;
                }
                else if (effect.type == Effect.BURN)
                {
                    if (Random.Range(0f, 1f) > burnChance) continue;
                }
                if (effect.hasDuration)
                {
                    bool foundDuplicate = false;
                    for (int i = 0; i < activeEffectWrappers.Count; i++)
                    {
                        if (effect.type == activeEffectWrappers[i].effect.type)
                        {
                            activeEffectWrappers[i].effect.duration = activeEffectWrappers[i].effect.maxDuration > 0 ? Mathf.Min(activeEffectWrappers[i].effect.duration + effect.duration, activeEffectWrappers[i].effect.maxDuration) : activeEffectWrappers[i].effect.duration + effect.duration;
                            if (activeEffectWrappers[i].effect.stackable && (activeEffectWrappers[i].effect.currentStacks < activeEffectWrappers[i].effect.maxStacks || activeEffectWrappers[i].effect.maxStacks < 0))
                            {
                                activeEffectWrappers[i].effect.currentStacks = Mathf.Min(activeEffectWrappers[i].effect.currentStacks + effect.currentStacks, activeEffectWrappers[i].effect.maxStacks);
                            }
                            foundDuplicate = true;
                            break;
                        }

                    }
                    if (!foundDuplicate)
                    {
                        activeEffectWrappers.Add(new AppliedEffectWrapper(effect));
                    }
                }
                else
                {
                    activeEffectWrappers.Add(new AppliedEffectWrapper(effect));
                }
            }
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void ToggleEffectVfxClientRpc(Effect effect, bool enableVFX)
        {
            if (effect == Effect.BURN)
            {
                if (burnVfx)
                {
                    if (enableVFX && !burnVfx.isPlaying)
                    {
                        var main = burnVfx.main;
                        main.loop = true;
                        burnVfx.Play();
                    }
                    else
                        burnVfx.Stop(true);
                }
            }
            else if (effect == Effect.FROST)
            {
                if (frostVfx)
                {
                    if (enableVFX && !frostVfx.isPlaying)
                    {
                        var main = frostVfx.main;
                        main.loop = true;
                        frostVfx.Play(true);
                    }
                    else
                        frostVfx.Stop(true);

                }
            }
        }
    }
}