using System.Collections;
using ShooterSurvival.Players;
using ShooterSurvival.Util;
using UnityEngine;

namespace ShooterSurvival.UI
{
    public class PlayerHitUI : MonoBehaviour
    {
        [SerializeField]
        private float fadeTime = 1f;
        [SerializeField]
        private float maxOpacity = 0.6f;

        CanvasGroup cgroup;
        float timer;
        Coroutine currentRoutine = null;
        Player localPlayer;
        bool gotPlayer = false;

        void Awake()
        {
            cgroup = GetComponent<CanvasGroup>();
        }

        void Start()
        {
            FindPlayer();
        }

        void Update()
        {
            if (!localPlayer && !gotPlayer) //gotPlayer boolean prevents regetting the player when it becomes null after disconnection
            {
                FindPlayer();
            }
        }

        private void FindPlayer()
        {
            var no = NetworkUtil.GetLocalPlayer();
            if (no)
            {
                localPlayer = no.GetComponent<Player>();
                localPlayer.onPlayerHit += ProcessHit;
                gotPlayer = true;
            }
        }

        void OnDisable()
        {
            localPlayer.onPlayerHit -= ProcessHit;
        }

        private void ProcessHit()
        {
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }
            cgroup.alpha = maxOpacity;
            timer = fadeTime;
            currentRoutine = StartCoroutine(Fade());
        }
        private IEnumerator Fade()
        {
            while (cgroup.alpha > 0)
            {
                timer -= Time.deltaTime;
                cgroup.alpha = Mathf.Clamp(cgroup.alpha, 0, timer / fadeTime);
                yield return null;
            }
            currentRoutine = null;
        }
    }
}