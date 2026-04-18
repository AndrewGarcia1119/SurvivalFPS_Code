using UnityEngine;

namespace ShooterSurvival.AI
{
	[CreateAssetMenu(fileName = "BodyPart", menuName = "Scriptable Objects/BodyPart")]
	public class BodyPart : ScriptableObject
	{
		[SerializeField]
		float damageMultiplier = 1f;

		internal float getDamageMultiplier()
		{
			return damageMultiplier;
		}

	}

}