using System;
using UnityEngine;

namespace ShooterSurvival.Players
{
    public class ReloadHandler : MonoBehaviour
    {
        [SerializeField]
        private Animator reloadAnimator;

        [SerializeField]
        private string idleAnimationName = "Idle", reloadAnimationName = "reload";

        private int idleAnimHash = -1, reloadAnimHash = -1;
        public float ReloadSpeed { get; set; } = 1f;

        void Awake()
        {
            idleAnimHash = Animator.StringToHash(idleAnimationName);
            reloadAnimHash = Animator.StringToHash(reloadAnimationName);
        }

        void Update()
        {
            reloadAnimator.speed = ReloadSpeed;
        }

        void OnEnable()
        {
            reloadAnimator.Play(idleAnimHash, 0, 0);
        }

        public void ReloadAnim()
        {

            reloadAnimator.Play(reloadAnimHash, 0, 0);
        }

        public void IdleAnim()
        {
            reloadAnimator.Play(idleAnimHash, 0, 0);
        }


    }
}
