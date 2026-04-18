using UnityEngine;
using ShooterSurvival.GameSystems;
using ShooterSurvival.Players;

namespace ShooterSurvival.AI
{
	public class EnemyBodyPart : MonoBehaviour, IDamagable
	{
		[SerializeField]
		private BodyPart bodyPartInfo;

		private Enemy thisCharacter;

		public void OnHit(float damage, ulong sourceClientId, WeaponUpgrades[] activeUpgrade = null, bool ignoreBodyPartMultipliers = false)
		{
			if (!thisCharacter)
				thisCharacter = GetComponentInParent<Enemy>();

			if (!ignoreBodyPartMultipliers)
			{
				thisCharacter.TakeDamageServerRpc(damage * bodyPartInfo.getDamageMultiplier(), sourceClientId);
			}
			else
			{
				thisCharacter.TakeDamageServerRpc(damage, sourceClientId);
			}
			EnemyEffectHandler enemyEffectHandler = GetComponentInParent<EnemyEffectHandler>();
			if (enemyEffectHandler && activeUpgrade != null)
			{
				AppliedEffect[] effects = new AppliedEffect[activeUpgrade.Length];
				for (int i = 0; i < activeUpgrade.Length; i++)
				{
					AppliedEffect appliedEffect = thisCharacter.gm.upgradeToAppliedEffect(activeUpgrade[i]);
					appliedEffect.sourceId = sourceClientId;
					effects[i] = appliedEffect;
				}
				enemyEffectHandler.ApplyEffect(effects);
			}
		}

		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Start()
		{
			thisCharacter = GetComponentInParent<Enemy>();
		}

	}

}