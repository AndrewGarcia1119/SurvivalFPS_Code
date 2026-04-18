using UnityEngine;
using ShooterSurvival.Players;
using System.Collections.Generic;

namespace ShooterSurvival.GameSystems
{
	public class BuyableRandomWeapon : MonoBehaviour, InteractiveObject
	{
		[SerializeField]
		private int price = 1500;
		[SerializeField]
		private float coolDownTimer = 3f;


		private string interactionText;
		GameManager gm;
		bool onCoolDown = false;

		private void Awake()
		{
			interactionText = $"press E to buy a random weapon ({price} points)";
		}

		// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
		private void Start()
		{
			gm = FindAnyObjectByType<GameManager>();
		}

		public void OnInteract(Interactor interactor, bool InteractedThisFrame)
		{
			if (!InteractedThisFrame) return;
			Player p = interactor.GetComponent<Player>();
			if (p.CanAfford(price))
			{
				WeaponHandler wh = interactor.GetComponent<WeaponHandler>();
				List<WeaponInfo> randomWeapons = GetAvailableWeapons(wh);
				int rand = Random.Range(0, randomWeapons.Count);
				p.SubtractPointsRpc(price);
				wh.AddWeapon(randomWeapons[rand]);
				interactor.RemoveAvailableInteractive();
				onCoolDown = true;
				Invoke(nameof(RemoveCooldownStatus), coolDownTimer);
			}
		}

		public string GetInteractionText()
		{
			return interactionText;
		}

		public bool AbleToInteract(Interactor interactor)
		{
			return !onCoolDown;
		}

		public InteractiveButton GetInteractiveButton()
		{
			return InteractiveButton.PRIMARY;
		}

		private void OnTriggerStay(Collider other)
		{
			if (other.TryGetComponent<Interactor>(out Interactor interactor))
			{
				if (AbleToInteract(interactor))
				{
					interactor.AddAvailableInteractive(this);
				}
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.TryGetComponent<Interactor>(out Interactor interactor))
			{
				interactor.RemoveAvailableInteractive();
			}
		}

		private void RemoveCooldownStatus()
		{
			onCoolDown = false;
		}

		private List<WeaponInfo> GetAvailableWeapons(WeaponHandler player)
		{
			List<WeaponInfo> availableWeapons = new(gm.GetAllWeapons());
			foreach (WeaponInfo wi in player.GetAllWeaponInfos())
			{
				availableWeapons.Remove(wi);
			}
			return availableWeapons;
		}
		public void ResetInteractive()
		{
			return;
		}
	}
}
