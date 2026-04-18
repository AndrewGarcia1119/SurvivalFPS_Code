using UnityEngine;

namespace ShooterSurvival.Players
{
    [RequireComponent(typeof(Animator))]
    public class BoltPumpHandler : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private float animationSpeed;
        [SerializeField]
        private AudioSource audioSource;
        [SerializeField]
        private string animName = "BoltPump";
        [SerializeField]
        private AudioClip pump1Audio, pump2Audio;

        public bool WeaponReady { get; set; }
        public bool BoltPumpActionInProgress { get; private set; }
        public float AnimationSpeedIncreaseMultiplier { get; set; } = 1f;

        void Start()
        {
            SetWeaponReady();
        }
        private void OnDisable()
        {
            animator.speed = 0;
            BoltPumpActionInProgress = false;
        }

        public void BoltPumpAction()
        {
            BoltPumpActionInProgress = true;
            animator.speed = animationSpeed * AnimationSpeedIncreaseMultiplier;
            animator.Play(animName, 0, 0);
        }

        public void PlayPump1Audio()
        {
            audioSource.clip = pump1Audio;
            audioSource.Play();
        }
        public void PlayPump2Audio()
        {
            audioSource.clip = pump2Audio;
            audioSource.Play();
        }

        public void SetWeaponReady()
        {
            WeaponReady = true;
            BoltPumpActionInProgress = false;
        }
    }
}