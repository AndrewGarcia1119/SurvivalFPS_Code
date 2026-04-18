using UnityEngine;

namespace ShooterSurvival.Players
{
    public class GunshotVFXPlayer : MonoBehaviour
    {
        [SerializeField]
        private GameObject vfxPrefab;
        [SerializeField]
        private GameObject vfxParent;
        [SerializeField]
        private float deleteTimer = 0.5f;

        public void SpawnVFX()
        {
            GameObject vfx = Instantiate(vfxPrefab, vfxParent.transform);
            DeleteVFXOnTimer(vfx);
        }

        private void DeleteVFXOnTimer(GameObject vfx)
        {
            Destroy(vfx, deleteTimer);
        }

        private void OnDisable()
        {
            foreach (Transform child in vfxParent.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

}