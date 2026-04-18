using UnityEngine;
using ShooterSurvival.GameSystems;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;

namespace ShooterSurvival.Players
{


	public class WeaponHandler : NetworkBehaviour
	{
		[System.Serializable]
		public class Weapon
		{
			public WeaponInfo weaponInfo;
			[HideInInspector]
			public int activeAmmoInMag;
			[HideInInspector]
			public int activeBackupAmmo;
			[HideInInspector]
			public GameObject weaponObject;
			[HideInInspector]
			public GunshotVFXPlayer vfxPlayer;
			[HideInInspector]
			public AnimatorOverrideController animController;
			[HideInInspector]
			public float currentWeaponDamage;
			[HideInInspector]
			public int currentWeaponLevel;
			[HideInInspector]
			public int maxWeaponlevel;
			//[HideInInspector]
			public List<WeaponUpgrades> currentUpgrades = new();
			[HideInInspector]
			public BoltPumpHandler boltPumpHandler = null;
			[HideInInspector]
			public ReloadHandler reloadHandler = null;
		}
		[SerializeField]
		Weapon[] startingWeapons;
		[SerializeField]
		int defaultMaxWeapons = 2;
		[SerializeField]
		float knifeDamageTimer = 0.1f;
		[SerializeField]
		float knifeCooldown = 0.4f;
		[SerializeField]
		AudioSource knifeAudioSource;
		[SerializeField]
		GameObject weaponSlot;
		[SerializeField]
		GameObject weaponWrapper;
		[SerializeField]
		float aimSpeed = 1f;

		//weapon upgrade modifiers
		[SerializeField]
		private float weaponDamageUpgradeMultiplier = 1.5f;
		[SerializeField]
		private int penetrationUpgradeCount = 2;

		PlayerInput playerInput;
		Camera cam;
		Melee knife;
		Animator aimAnimator;

		int activeWeaponIndex = 0;
		float weaponTimer = 0f;
		bool reloadingWeapon = false;
		string aimAnimName = "Aim";
		string unaimAnimName = "Unaim";
		int aimAnimHash;
		int unaimAnimHash;
		Coroutine activeReloadCoroutine;
		Coroutine activeBurstCoroutine;
		bool isAiming = false;

		GameManager gm;
		PauseManager pm;

		//[HideInInspector]
		public List<Weapon> currentWeapons;
		public bool IsAiming { get { return isAiming; } }

		public bool ReloadingWeapon { get { return reloadingWeapon; } }

		//perk modifiers
		[HideInInspector]
		public NetworkVariable<float> personalDamageMultiplier = new NetworkVariable<float>();
		[HideInInspector]
		public NetworkVariable<float> personalReloadSpeedDivider = new NetworkVariable<float>();
		[HideInInspector]
		public NetworkVariable<float> personalShootSpeedMultiplier = new NetworkVariable<float>();
		[HideInInspector]
		public NetworkVariable<int> currentMaxWeapons = new NetworkVariable<int>();

		public event Action onWeaponUpgradesChanged;


		// Awake is called when the script instance is being loaded.
		protected void Awake()
		{
			currentWeapons = new(startingWeapons);
			playerInput = new PlayerInput();
			gm = FindAnyObjectByType<GameManager>();
		}
		protected void OnEnable()
		{
			ToggleInput(true);
		}

		public override void OnNetworkSpawn()
		{
			if (IsServer)
			{
				personalDamageMultiplier.Value = 1;
				personalReloadSpeedDivider.Value = 1;
				personalShootSpeedMultiplier.Value = 1;
				currentMaxWeapons.Value = defaultMaxWeapons;
			}
			if (IsOwner)
			{
				knife = GetComponentInChildren<Melee>(true);
				if (knife.isActiveAndEnabled)
				{
					gameObject.SetActive(false);
				}
			}
		}
		private void Start()
		{
			pm = FindAnyObjectByType<PauseManager>();
			cam = GetComponentInChildren<Camera>();
			for (int i = 0; i < currentWeapons.Count; i++)
			{
				currentWeapons[i].activeAmmoInMag = currentWeapons[i].weaponInfo.GetAmmoPerMag();
				currentWeapons[i].activeBackupAmmo = currentWeapons[i].weaponInfo.GetStartingAmmoCount();
				currentWeapons[i].animController = currentWeapons[i].weaponInfo.GetAnimController();
				currentWeapons[i].currentWeaponDamage = currentWeapons[i].weaponInfo.GetWeaponDamage();
				currentWeapons[i].currentWeaponLevel = 0;
				currentWeapons[i].maxWeaponlevel = currentWeapons[i].weaponInfo.GetUpgradeTiers().Count;
				foreach (var slot in currentWeapons[i].weaponInfo.GetUpgradeTiers())
				{
					currentWeapons[i].currentUpgrades.Add(WeaponUpgrades.NONE);
				}
				TrySpawnWeaponPrefab(activeWeaponIndex);
				if (i > 0 && currentWeapons[i].weaponInfo.GetWeaponPrefab())
				{
					currentWeapons[i].weaponObject.SetActive(false);
				}
			}
			aimAnimator = weaponSlot.GetComponent<Animator>();
			aimAnimator.runtimeAnimatorController = currentWeapons[activeWeaponIndex].animController;
			aimAnimHash = Animator.StringToHash(aimAnimName);
			unaimAnimHash = Animator.StringToHash(unaimAnimName);
			if (aimAnimator.runtimeAnimatorController)
			{
				aimAnimator.Play(unaimAnimHash, 0, 0);
			}
		}

		// Update is called once per frame
		private void Update()
		{
			if (!IsOwner) return;
			if (pm && pm.IsLocalPlayerPaused) return;
			if (currentWeapons.Count == 0) return;
			weaponTimer += Time.deltaTime;

			if (playerInput.Player.Melee.WasPressedThisFrame() && !reloadingWeapon /*remove this once knife animation is in, it should be able to cancel reload*/)
			{
				HandleSwitchWeapon();
				KnifeAttack();
			}
			if (aimAnimator)
			{
				aimAnimator.speed = aimSpeed;
				if (playerInput.Player.Aim.IsPressed() && !reloadingWeapon)
				{
					isAiming = true;
					if (aimAnimator.runtimeAnimatorController && aimAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash != aimAnimHash)
					{
						aimAnimator.Play(aimAnimHash, 0, Mathf.Max(1 - aimAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime, 0));
					}
				}
				else
				{
					isAiming = false;
					if (aimAnimator.runtimeAnimatorController && aimAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash != unaimAnimHash)
					{
						aimAnimator.Play(unaimAnimHash, 0, Mathf.Max(1 - aimAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime, 0));
					}
				}
			}
			WeaponInfo wi = currentWeapons[activeWeaponIndex].weaponInfo;
			if (currentWeapons[activeWeaponIndex].activeAmmoInMag > 0 && !reloadingWeapon)
			{
				if ((wi.GetWeaponType() == WeaponType.SHOTGUN && wi.IsPumpAction()) || wi.GetWeaponType() == WeaponType.BOLT)
				{
					var bpHandler = currentWeapons[activeWeaponIndex].boltPumpHandler;
					if (weaponTimer >= wi.GetBoltPumpActionCooltime() && bpHandler && !bpHandler.WeaponReady && !bpHandler.BoltPumpActionInProgress)
					{
						bpHandler.AnimationSpeedIncreaseMultiplier = 1f / personalShootSpeedMultiplier.Value;
						bpHandler.BoltPumpAction();
					}
				}
				if (weaponTimer >= wi.GetRateOfFire() * personalShootSpeedMultiplier.Value && BoltOrPumpReady())
				{
					if (wi.GetWeaponType() == WeaponType.SEMI_AUTOMATIC)
					{
						if (playerInput.Player.Attack.WasPressedThisFrame())
						{
							UseWeapon(true);
							return;
						}
					} //Add checks for other weapon types
					else if (wi.GetWeaponType() == WeaponType.BURST)
					{
						if (playerInput.Player.Attack.WasPressedThisFrame())
						{
							activeBurstCoroutine = StartCoroutine(UseBurstWeapon());
							return;
						}
					}
					else if (wi.GetWeaponType() == WeaponType.SHOTGUN)
					{
						if (playerInput.Player.Attack.WasPressedThisFrame())
						{
							if (currentWeapons[activeWeaponIndex].boltPumpHandler)
							{
								currentWeapons[activeWeaponIndex].boltPumpHandler.WeaponReady = false;
							}
							for (int i = 0; i < wi.GetNumPellets(); i++)
							{
								Vector2 point = UnityEngine.Random.insideUnitCircle;

								// Scale into ellipse
								float horizontal = point.x * wi.GetPelletSpread().x;
								float vertical = point.y * wi.GetPelletSpread().y;

								Quaternion rotation = Quaternion.Euler(vertical, horizontal, 0f);
								Ray startingRay = GetMiddleOfScreen();
								startingRay.direction = rotation * startingRay.direction;
								if (i == 0)
								{
									UseWeapon(true, startingRay);
								}
								else
								{
									UseWeapon(false, startingRay, false, false, false);
								}
							}

							return;
						}
					}
					else
					{
						if (playerInput.Player.Attack.IsPressed())
						{
							UseWeapon(true);
							return;
						}
					}
				}
			}
			if (!reloadingWeapon && currentWeapons[activeWeaponIndex].activeBackupAmmo > 0 && currentWeapons[activeWeaponIndex].activeAmmoInMag < (wi.GetAmmoPerMag() + GetBonusMagAmmoForWeapon(activeWeaponIndex)) && activeBurstCoroutine == null && (playerInput.Player.Reload.IsPressed() || (playerInput.Player.Attack.IsPressed() && currentWeapons[activeWeaponIndex].activeAmmoInMag <= 0)))
			{
				activeReloadCoroutine = StartCoroutine(ReloadWeapon(activeWeaponIndex));
			}
			if (playerInput.Player.Next.WasPressedThisFrame())
			{
				ResetWeaponPos();
				SwitchWeaponClientRpc(false);
				HandleSwitchWeapon();
			}
			else if (playerInput.Player.Previous.WasPressedThisFrame())
			{
				ResetWeaponPos();
				SwitchWeaponClientRpc(true);
				HandleSwitchWeapon();
			}
		}

		private bool BoltOrPumpReady()
		{
			if (!currentWeapons[activeWeaponIndex].weaponInfo.IsPumpAction() || !currentWeapons[activeWeaponIndex].boltPumpHandler) return true;
			return currentWeapons[activeWeaponIndex].boltPumpHandler.WeaponReady;
		}
		[Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Owner)]
		private void SwitchWeaponClientRpc(bool gotoPrevious)
		{
			if (!gotoPrevious)
			{
				activeWeaponIndex = (activeWeaponIndex + 1) % currentWeapons.Count;
			}
			else
			{
				activeWeaponIndex = (activeWeaponIndex - 1) % currentWeapons.Count;
				if (activeWeaponIndex < 0)
					activeWeaponIndex = currentWeapons.Count - 1;
			}
		}

		private void ResetWeaponPos()
		{
			weaponWrapper.transform.localPosition = currentWeapons[activeWeaponIndex].weaponInfo.GetRelativePosition();
			weaponWrapper.transform.rotation = currentWeapons[activeWeaponIndex].weaponInfo.GetRotation();
		}

		private void HandleSwitchWeapon()
		{
			weaponTimer = 0f;
			CancelReload();
			CancelBurst();
			aimAnimator.runtimeAnimatorController = currentWeapons[activeWeaponIndex].animController;
			weaponWrapper.transform.localPosition = currentWeapons[activeWeaponIndex].weaponInfo.GetRelativePosition();
			weaponWrapper.transform.localRotation = currentWeapons[activeWeaponIndex].weaponInfo.GetRotation();
			SetCurrentWeaponActiveClientRpc();
		}

		private void UseWeapon(bool resetWeaponTimer, Ray? direction = null, bool produceSound = true, bool produceVFX = true, bool useBullet = true)
		{
			if (resetWeaponTimer)
			{
				weaponTimer = 0f;
			}
			if (useBullet)
			{
				currentWeapons[activeWeaponIndex].activeAmmoInMag--;
			}
			if (produceSound)
			{
				PlayWeaponSoundClientRpc(activeWeaponIndex);
			}
			if (produceVFX)
			{
				PlayWeaponVFXClientRpc(activeWeaponIndex);
			}
			if (!currentWeapons[activeWeaponIndex].weaponInfo.CanWeaponPierce() && !currentWeapons[activeWeaponIndex].currentUpgrades.Contains(WeaponUpgrades.PENETRATE_BONUS))
			{
				if (Physics.Raycast(!direction.HasValue ? GetMiddleOfScreen() : direction.Value, out RaycastHit hit, currentWeapons[activeWeaponIndex].weaponInfo.GetShootDistance()))
				{
					if (hit.transform.TryGetComponent<IDamagable>(out IDamagable shootable))
					{
						if (!gm.isFriendlyFireAllowed && hit.transform.GetComponent<Player>()) return;
						float damage = currentWeapons[activeWeaponIndex].currentWeaponDamage * personalDamageMultiplier.Value;
						if (currentWeapons[activeWeaponIndex].currentUpgrades.Contains(WeaponUpgrades.INCREASED_DAMAGE))
						{
							damage *= weaponDamageUpgradeMultiplier;
						}
						shootable.OnHit(damage, OwnerClientId, currentWeapons[activeWeaponIndex].currentUpgrades.ToArray());
					}

				}
			}
			else
			{
				//This occurs if the weapon can penetrate through enemies
				var hits = Physics.RaycastAll(!direction.HasValue ? GetMiddleOfScreen() : direction.Value, currentWeapons[activeWeaponIndex].weaponInfo.GetShootDistance());
				Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
				int hitsLeft = (currentWeapons[activeWeaponIndex].currentUpgrades.Contains(WeaponUpgrades.PENETRATE_BONUS) ? penetrationUpgradeCount : 0) + currentWeapons[activeWeaponIndex].weaponInfo.GetDefaultPierceBonus() + 1;
				foreach (var hit in hits)
				{
					if (hitsLeft <= 0) break;
					if (hit.transform.TryGetComponent<IDamagable>(out IDamagable shootable))
					{
						if (!gm.isFriendlyFireAllowed && hit.transform.GetComponent<Player>()) return;
						hitsLeft--;
						float damage = currentWeapons[activeWeaponIndex].currentWeaponDamage * personalDamageMultiplier.Value;
						float totalDistance = currentWeapons[activeWeaponIndex].weaponInfo.GetShootDistance();
						if (currentWeapons[activeWeaponIndex].currentUpgrades.Contains(WeaponUpgrades.INCREASED_DAMAGE))
						{
							damage *= weaponDamageUpgradeMultiplier;
						}
						shootable.OnHit(damage, OwnerClientId, currentWeapons[activeWeaponIndex].currentUpgrades.ToArray());
					}
					else
					{
						break;
					}
				}


			}


		}

		private IEnumerator UseBurstWeapon()
		{
			weaponTimer = 0f;
			int index = activeWeaponIndex;
			WeaponInfo wi = currentWeapons[activeWeaponIndex].weaponInfo;
			for (int i = 0; i < wi.GetBurstCount(); i++)
			{
				UseWeapon(false);
				if (index != activeWeaponIndex || currentWeapons[activeWeaponIndex].activeAmmoInMag <= 0)
				{
					break;
				}
				if (i != wi.GetBurstCount() - 1)
				{
					yield return new WaitForSeconds(wi.GetBurstShotSpeed() * personalShootSpeedMultiplier.Value);
				}
			}
			activeBurstCoroutine = null;
		}
		private void CancelBurst()
		{
			if (activeBurstCoroutine != null)
			{
				StopCoroutine(activeBurstCoroutine);
			}
			activeBurstCoroutine = null;
		}

		[Rpc(SendTo.ClientsAndHost)]
		private void PlayWeaponSoundClientRpc(int index)
		{
			AudioSource.PlayClipAtPoint(currentWeapons[index].weaponInfo.GetWeaponSound(), transform.position);
		}

		[Rpc(SendTo.ClientsAndHost)]
		private void PlayWeaponVFXClientRpc(int index)
		{
			if (currentWeapons[index].vfxPlayer)
			{
				currentWeapons[index].vfxPlayer.SpawnVFX();
			}
		}

		private IEnumerator ReloadWeapon(int reloadIndex)
		{
			reloadingWeapon = true;
			WeaponInfo wi = currentWeapons[reloadIndex].weaponInfo;
			if (currentWeapons[reloadIndex].reloadHandler)
			{
				currentWeapons[reloadIndex].reloadHandler.ReloadAnim();
				currentWeapons[reloadIndex].reloadHandler.ReloadSpeed = personalReloadSpeedDivider.Value;
			}
			yield return new WaitForSeconds(wi.GetReloadSpeed() / personalReloadSpeedDivider.Value);
			int totalToAdd = Mathf.Min(wi.GetAmmoPerMag() + GetBonusMagAmmoForWeapon(reloadIndex) - currentWeapons[reloadIndex].activeAmmoInMag, currentWeapons[reloadIndex].activeBackupAmmo);
			currentWeapons[reloadIndex].activeBackupAmmo -= totalToAdd;
			currentWeapons[reloadIndex].activeAmmoInMag += totalToAdd;
			reloadingWeapon = false;

		}

		private void CancelReload()
		{
			if (activeReloadCoroutine != null)
			{
				StopCoroutine(activeReloadCoroutine);
			}
			activeReloadCoroutine = null;
			reloadingWeapon = false;
		}



		// This function is called when the behaviour becomes disabled () or inactive.
		protected void OnDisable()
		{
			ToggleInput(false);
		}

		private Ray GetMiddleOfScreen()
		{
			if (!cam)
				cam = GetComponentInChildren<Camera>();
			return cam.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
		}

		[Rpc(SendTo.ClientsAndHost)]
		private void AddWeaponRpc(int index)
		{
			Weapon w = new();
			WeaponInfo wi = gm.GetWeaponInfoAt(index);
			w.weaponInfo = wi;
			w.activeAmmoInMag = wi.GetAmmoPerMag();
			w.activeBackupAmmo = wi.GetStartingAmmoCount();
			w.animController = wi.GetAnimController();
			w.currentWeaponDamage = wi.GetWeaponDamage();
			w.currentWeaponLevel = 0;
			w.maxWeaponlevel = wi.GetUpgradeTiers().Count;
			if (currentWeapons.Count < currentMaxWeapons.Value)
			{
				currentWeapons.Add(w);
				activeWeaponIndex = currentWeapons.Count - 1;
				currentWeapons[activeWeaponIndex].activeAmmoInMag = currentWeapons[activeWeaponIndex].weaponInfo.GetAmmoPerMag();
				currentWeapons[activeWeaponIndex].activeBackupAmmo = currentWeapons[activeWeaponIndex].weaponInfo.GetStartingAmmoCount();
				foreach (var slot in currentWeapons[activeWeaponIndex].weaponInfo.GetUpgradeTiers())
				{
					currentWeapons[activeWeaponIndex].currentUpgrades.Add(WeaponUpgrades.NONE);
				}
				aimAnimator.runtimeAnimatorController = currentWeapons[activeWeaponIndex].animController;
				weaponWrapper.transform.SetLocalPositionAndRotation(currentWeapons[activeWeaponIndex].weaponInfo.GetRelativePosition(), currentWeapons[activeWeaponIndex].weaponInfo.GetRotation());
				TrySpawnWeaponPrefab(activeWeaponIndex);
			}
			else
			{
				TryRemoveWeaponPrefab(index);
				currentWeapons.Insert(activeWeaponIndex, w);
				currentWeapons.RemoveAt(activeWeaponIndex + 1);
				TrySpawnWeaponPrefab(activeWeaponIndex);
				aimAnimator.runtimeAnimatorController = currentWeapons[activeWeaponIndex].animController;
				foreach (var slot in currentWeapons[activeWeaponIndex].weaponInfo.GetUpgradeTiers())
				{
					currentWeapons[activeWeaponIndex].currentUpgrades.Add(WeaponUpgrades.NONE);
				}
				weaponWrapper.transform.SetLocalPositionAndRotation(currentWeapons[activeWeaponIndex].weaponInfo.GetRelativePosition(), currentWeapons[activeWeaponIndex].weaponInfo.GetRotation());
				onWeaponUpgradesChanged?.Invoke();
			}
			SetCurrentWeaponActive();
		}

		private void SetCurrentWeaponActive()
		{
			for (int i = 0; i < currentWeapons.Count; i++)
			{
				if (!currentWeapons[i].weaponObject) continue;
				currentWeapons[i].weaponObject.SetActive(i == activeWeaponIndex);

				if (aimAnimator.runtimeAnimatorController)
				{
					aimAnimator.Play(unaimAnimHash, 0, 1.0f);
				}
			}
		}

		[Rpc(SendTo.ClientsAndHost)]
		private void SetCurrentWeaponActiveClientRpc()
		{
			SetCurrentWeaponActive();
		}
		private void KnifeAttack()
		{
			StartCoroutine(KnifeSequence());
		}
		private IEnumerator KnifeSequence()
		{
			ToggleInput(false);
			if (knifeAudioSource)
			{
				knifeAudioSource.Play();
			}
			knife.gameObject.SetActive(true);
			yield return new WaitForSeconds(knifeDamageTimer);
			knife.gameObject.SetActive(false);
			yield return new WaitForSeconds(knifeCooldown - knifeDamageTimer);
			ToggleInput(true);
		}

		public void ToggleInput(bool enableInput)
		{
			if (enableInput)
			{
				playerInput.Player.Attack.Enable();
				playerInput.Player.Next.Enable();
				playerInput.Player.Previous.Enable();
				playerInput.Player.Reload.Enable();
				playerInput.Player.Melee.Enable();
				playerInput.Player.Aim.Enable();
			}
			else
			{
				playerInput.Player.Attack.Disable();
				playerInput.Player.Next.Disable();
				playerInput.Player.Previous.Disable();
				playerInput.Player.Reload.Disable();
				playerInput.Player.Melee.Disable();
				playerInput.Player.Aim.Disable();
			}
		}

		private void TrySpawnWeaponPrefab(int index)
		{
			if (currentWeapons[index].weaponInfo.GetWeaponPrefab())
			{
				currentWeapons[index].weaponObject = Instantiate(
					currentWeapons[index].weaponInfo.GetWeaponPrefab(),
					weaponWrapper.transform, false
				);
				if (currentWeapons[index].weaponObject.TryGetComponent<BoltPumpHandler>(out BoltPumpHandler boltPumpHandler))
				{
					if (currentWeapons[index].weaponInfo.GetWeaponType() == WeaponType.SHOTGUN || currentWeapons[index].weaponInfo.GetWeaponType() == WeaponType.BOLT)
					{
						currentWeapons[index].boltPumpHandler = boltPumpHandler;
					}
				}
				if (currentWeapons[index].weaponObject.TryGetComponent<ReloadHandler>(out ReloadHandler reloadHandler))
				{
					currentWeapons[index].reloadHandler = reloadHandler;
				}
				//some guns (usually with scope) have camera components, make sure to disable these if the camera not on the owner player
				if (!IsOwner && currentWeapons[index].weaponObject)
				{
					Camera c = currentWeapons[index].weaponObject.GetComponentInChildren<Camera>();
					if (c)
					{
						c.gameObject.SetActive(false);
					}
				}


				currentWeapons[index].vfxPlayer = currentWeapons[index].weaponObject.GetComponent<GunshotVFXPlayer>();
				weaponWrapper.transform.localPosition = currentWeapons[activeWeaponIndex].weaponInfo.GetRelativePosition();
				weaponWrapper.transform.localRotation = currentWeapons[activeWeaponIndex].weaponInfo.GetRotation();
			}
		}
		private void TryRemoveWeaponPrefab(int index)
		{
			if (currentWeapons[activeWeaponIndex].weaponObject)
			{
				Destroy(currentWeapons[activeWeaponIndex].weaponObject);
			}
		}

		[Rpc(SendTo.Owner)]
		public void ResetAmmoRpc(bool currentWeaponOnly = false)
		{
			if (currentWeaponOnly)
			{
				currentWeapons[activeWeaponIndex].activeAmmoInMag = currentWeapons[activeWeaponIndex].weaponInfo.GetAmmoPerMag() + GetBonusMagAmmoForWeapon(activeWeaponIndex);
				currentWeapons[activeWeaponIndex].activeBackupAmmo = currentWeapons[activeWeaponIndex].weaponInfo.GetTotalAmmoCount() + GetBonusTotalAmmoForWeapon(activeWeaponIndex);
				return;
			}
			for (int i = 0; i < currentWeapons.Count; i++)
			{
				currentWeapons[i].activeAmmoInMag = currentWeapons[i].weaponInfo.GetAmmoPerMag() + GetBonusMagAmmoForWeapon(i);
				currentWeapons[i].activeBackupAmmo = currentWeapons[i].weaponInfo.GetTotalAmmoCount() + GetBonusTotalAmmoForWeapon(i);
			}
		}

		private int GetBonusMagAmmoForWeapon(int index) // Only gets bonus ammo if it has the increased ammo upgrade
		{
			return !currentWeapons[index].currentUpgrades.Contains(WeaponUpgrades.INCREASED_AMMO) ? 0 : currentWeapons[index].weaponInfo.GetBonusAmmoMagIncrease();

		}
		private int GetBonusTotalAmmoForWeapon(int index) // Only gets bonus ammo if it has the increased ammo upgrade
		{
			return !currentWeapons[index].currentUpgrades.Contains(WeaponUpgrades.INCREASED_AMMO) ? 0 : currentWeapons[index].weaponInfo.GetBonusTotalAmmoIncrease();

		}
		public int GetAmmoInCurrentWeapon()
		{
			return currentWeapons[activeWeaponIndex].activeAmmoInMag;
		}
		public int GetBackupAmmoInCurrentWeapon()
		{
			return currentWeapons[activeWeaponIndex].activeBackupAmmo;
		}
		public int GetCurrentWeaponAmmoRefillCost()
		{
			var weapon = currentWeapons[activeWeaponIndex];
			if (weapon.currentWeaponLevel <= 0)
			{
				return weapon.weaponInfo.GetRefillAmmoCost();
			}
			return weapon.weaponInfo.GetUpgradeTiers()[weapon.currentWeaponLevel - 1].ammoRefillCostOverride;
		}

		public bool HasFullAmmo()
		{
			if (GetAmmoInCurrentWeapon() < currentWeapons[activeWeaponIndex].weaponInfo.GetAmmoPerMag() + GetBonusMagAmmoForWeapon(activeWeaponIndex))
			{
				return false;
			}
			if (GetBackupAmmoInCurrentWeapon() < currentWeapons[activeWeaponIndex].weaponInfo.GetTotalAmmoCount() + GetBonusTotalAmmoForWeapon(activeWeaponIndex))
			{
				return false;
			}
			return true;
		}
		public string GetCurrentWeaponName()
		{
			return currentWeapons[activeWeaponIndex].weaponInfo.GetWeaponName();
		}

		public List<WeaponUpgrades> GetCurrentWeaponUpgrades()
		{
			return currentWeapons[activeWeaponIndex].currentUpgrades;
		}

		public int GetCurrentWeaponIndex()
		{
			return activeWeaponIndex;
		}
		public bool HasWeapon(WeaponInfo wi)
		{
			foreach (Weapon w in currentWeapons)
			{
				if (w.weaponInfo == wi)
				{
					return true;
				}
			}
			return false;
		}
		public void AddWeapon(WeaponInfo wi)
		{
			int index = gm.GetIndexOfWeapon(wi);
			if (index < 0) return;
			CancelReload();
			AddWeaponRpc(index);

		}

		public ReadOnlyCollection<WeaponInfo> GetAllWeaponInfos()
		{
			List<WeaponInfo> weaponInfos = new();
			foreach (Weapon w in currentWeapons)
			{
				weaponInfos.Add(w.weaponInfo);
			}
			return weaponInfos.AsReadOnly();
		}
		public void UpgradeWeapon()
		{
			if (currentWeapons[activeWeaponIndex].currentWeaponLevel < currentWeapons[activeWeaponIndex].maxWeaponlevel)
			{
				int level = currentWeapons[activeWeaponIndex].currentWeaponLevel++;
				var upgrade = currentWeapons[activeWeaponIndex].weaponInfo.GetUpgradeTiers()[level];
				currentWeapons[activeWeaponIndex].currentWeaponDamage = currentWeapons[activeWeaponIndex].weaponInfo.GetUpgradeTiers()[level].damage;
				var upgradeInfoSlot = currentWeapons[activeWeaponIndex].weaponInfo.GetAvailableUpgradeSlots()[upgrade.upgradeSlotIndex];
				if (upgradeInfoSlot.Count > 0)
				{
					int randUpgradeIndex = UnityEngine.Random.Range(0, upgradeInfoSlot.Count);
					currentWeapons[activeWeaponIndex].currentUpgrades[upgrade.upgradeSlotIndex] = upgradeInfoSlot[randUpgradeIndex];
				}
				currentWeapons[activeWeaponIndex].activeAmmoInMag = currentWeapons[activeWeaponIndex].weaponInfo.GetAmmoPerMag() + GetBonusMagAmmoForWeapon(activeWeaponIndex);
				currentWeapons[activeWeaponIndex].activeBackupAmmo = currentWeapons[activeWeaponIndex].weaponInfo.GetTotalAmmoCount() + GetBonusTotalAmmoForWeapon(activeWeaponIndex);
			}
			else
			{
				int randIndex = UnityEngine.Random.Range(0, currentWeapons[activeWeaponIndex].weaponInfo.GetAvailableUpgradeSlots().Count);
				var upgrade = currentWeapons[activeWeaponIndex].weaponInfo.GetUpgradeTiers()[randIndex];
				var upgradeInfoSlot = currentWeapons[activeWeaponIndex].weaponInfo.GetAvailableUpgradeSlots()[upgrade.upgradeSlotIndex];

				if (upgradeInfoSlot.Count > 0)
				{
					List<WeaponUpgrades> upgradesCopy = new(upgradeInfoSlot);
					upgradesCopy.Remove(currentWeapons[activeWeaponIndex].currentUpgrades[upgrade.upgradeSlotIndex]);
					int randUpgradeIndex = UnityEngine.Random.Range(0, upgradesCopy.Count);
					currentWeapons[activeWeaponIndex].currentUpgrades[upgrade.upgradeSlotIndex] = upgradesCopy[randUpgradeIndex];
				}
			}
			onWeaponUpgradesChanged?.Invoke();
		}

		public bool AbleToUpgradeActiveWeapon()
		{
			return currentWeapons[activeWeaponIndex].maxWeaponlevel > 0;
		}
		public int ActiveWeaponLevel()
		{
			return currentWeapons[activeWeaponIndex].currentWeaponLevel;
		}
		public int ActiveWeaponMaxLevel()
		{
			return currentWeapons[activeWeaponIndex].maxWeaponlevel;
		}

		public void HandlePlayerDied()
		{
			ToggleInput(false);
		}
		public void HandlePlayerRevived()
		{
			ToggleInput(true);
		}

	}

}