using UnityEngine;

[CreateAssetMenu(fileName = "RoundInfo", menuName = "Scriptable Objects/RoundInfo")]
public class RoundInfo : ScriptableObject
{
	[SerializeField]
	private float healthMultiplier = 1.1f;
	[Tooltip("Additive applies after multiplication")]
	[SerializeField]
	private float healthAdditive = 0;
	[SerializeField]
	private float enemyCountMultiplier = 1.1f;
	[Tooltip("Additive applies after multiplication")]
	[SerializeField]
	private int enemyCountAdditive = 0;
	[SerializeField]
	private int totalPossiblePowerupsInRound = 4;

	public float GetHealthMultiplier()
	{
		return healthMultiplier;
	}

	public float GetHealthAdditive()
	{
		return healthAdditive;
	}

	public float GetEnemyCountMultiplier()
	{
		return enemyCountMultiplier;
	}

	public int GetEnemyCountAdditive()
	{
		return enemyCountAdditive;
	}

	public int GetTotalPossiblePowerupsInRound()
	{
		return totalPossiblePowerupsInRound;
	}
}
