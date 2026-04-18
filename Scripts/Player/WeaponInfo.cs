using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ShooterSurvival.Players
{
    [CreateAssetMenu(fileName = "WeaponInfo", menuName = "Scriptable Objects/WeaponInfo")]
    public class WeaponInfo : ScriptableObject
    {
        [System.Serializable]
        public class UpgradeInfoSlot : List<WeaponUpgrades>, ISerializationCallbackReceiver
        {
            [SerializeField]
            List<WeaponUpgrades> possibleWeaponUpgrades = new List<WeaponUpgrades>();

            public void OnBeforeSerialize()
            {
                possibleWeaponUpgrades.Clear();
                foreach (WeaponUpgrades wu in this)
                {
                    possibleWeaponUpgrades.Add(wu);
                }
            }

            public void OnAfterDeserialize()
            {
                Clear();
                foreach (WeaponUpgrades wu in possibleWeaponUpgrades)
                {
                    Add(wu);
                }
            }

        }

        [System.Serializable]
        public struct UpgradeTier
        {
            [Tooltip("New damage for weapon")]
            public float damage;
            [Tooltip("Upgrade slot to update, an upgrade slot index less than 0 should allow for a random slot to be updated")]
            public int upgradeSlotIndex;
            [Tooltip("Cost of refilling ammo")]
            public int ammoRefillCostOverride;
        }


        [SerializeField]
        string weaponName = "Weapon";
        [SerializeField]
        private float rateOfFire = 0.3f;
        [SerializeField]
        private float reloadSpeed = 1.5f;
        [SerializeField]
        private float weaponDamage = 13f;
        [SerializeField]
        private AudioClip weaponSound = null;
        [SerializeField]
        private GameObject weaponPrefab;
        [SerializeField]
        [Tooltip("Position relative to camera")]
        private Vector3 relativePosition;
        [SerializeField]
        [Tooltip("Rotation")]
        private Quaternion rotation;
        [SerializeField]
        private AnimatorOverrideController animController;
        [SerializeField]
        private List<UpgradeTier> upgradeTiers = new List<UpgradeTier>();
        [SerializeField]
        private float shootDistance = 50f;
        [SerializeField]
        private bool canWeaponPierce = false;
        [SerializeField]
        private int defaultPierceBonus = 1;
        public List<UpgradeInfoSlot> availableUpgradeSlots = new List<UpgradeInfoSlot>();


        [SerializeField]
        private int totalAmmoCount = 80;
        [SerializeField]
        private int ammoPerMag = 8;
        [SerializeField]
        private int startingAmmoCount = 32;
        [SerializeField]
        private int bonusAmmoMagIncrease = 4;
        [SerializeField]
        private int bonusTotalAmmoIncrease = 40;
        [SerializeField]
        private int refillAmmoCost = 600;


        [SerializeField]
        private WeaponType weaponType = WeaponType.SEMI_AUTOMATIC;

        [SerializeField]
        [Tooltip("Determines the time between each shot in a burst shot. ONLY USE THIS FOR BURST SHOT WEAPONS")]
        private float burstShotSpeed = 0.08f;
        [SerializeField]
        [Tooltip("Determines the number of shots in a single burst. ONLY USE THIS FOR BURST SHOT WEAPONS")]
        private int burstCount = 3;

        [SerializeField]
        [Tooltip("total number of pellets the shotgun shoots out.")]
        private int numPellets = 10;
        [SerializeField]
        [Tooltip("total angle from center of the screen outwards in degrees from which the pellets can spread")]
        private Vector2 pelletSpread = new Vector2(30, 30);
        [SerializeField]
        private bool isPumpAction = false;
        [SerializeField]
        private float pumpActionCooltime = 0.1f;

        public string GetWeaponName()
        {
            return weaponName;
        }

        internal WeaponType GetWeaponType()
        {
            return weaponType;
        }

        internal float GetRateOfFire()
        {
            return rateOfFire;
        }

        internal float GetBurstShotSpeed()
        {
            return burstShotSpeed;
        }

        internal float GetBurstCount()
        {
            return burstCount;
        }

        internal float GetReloadSpeed()
        {
            return reloadSpeed;
        }

        internal int GetTotalAmmoCount()
        {
            return totalAmmoCount;
        }

        internal int GetAmmoPerMag()
        {
            return ammoPerMag;
        }

        internal int GetBonusAmmoMagIncrease()
        {
            return bonusAmmoMagIncrease;
        }

        internal int GetBonusTotalAmmoIncrease()
        {
            return bonusTotalAmmoIncrease;
        }

        internal int GetStartingAmmoCount()
        {
            return startingAmmoCount;
        }

        internal float GetWeaponDamage()
        {
            return weaponDamage;
        }

        internal AudioClip GetWeaponSound()
        {
            return weaponSound;
        }

        internal GameObject GetWeaponPrefab()
        {
            return weaponPrefab;
        }

        internal Vector3 GetRelativePosition()
        {
            return relativePosition;
        }

        internal Quaternion GetRotation()
        {
            return rotation;
        }

        internal AnimatorOverrideController GetAnimController()
        {
            return animController;
        }

        internal List<UpgradeTier> GetUpgradeTiers()
        {
            return upgradeTiers;
        }

        internal List<UpgradeInfoSlot> GetAvailableUpgradeSlots()
        {
            return availableUpgradeSlots;
        }

        internal bool CanWeaponPierce()
        {
            return canWeaponPierce;
        }

        /// <summary>
        /// gets the default amount of enemies weapon can pierce through. RETURNS 0 if weaponCanPierce is marked as false
        /// </summary>
        /// <returns> number of enemies that the weapon can pierce through (always 0 if canWeaponPierce is false)</returns>
        internal int GetDefaultPierceBonus()
        {
            return canWeaponPierce ? defaultPierceBonus : 0;
        }

        internal int GetNumPellets()
        {
            return numPellets;
        }

        internal Vector2 GetPelletSpread()
        {
            return pelletSpread;
        }

        internal float GetShootDistance()
        {
            return shootDistance;
        }

        internal bool IsPumpAction()
        {
            return isPumpAction;
        }
        internal float GetBoltPumpActionCooltime()
        {
            return pumpActionCooltime;
        }
        internal int GetRefillAmmoCost()
        {
            return refillAmmoCost;
        }
    }

}