using System.Collections;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    [SerializeField]
    private float fadeTime = 1f;
    [SerializeField]
    private float fadeDelay = 1f;

    CanvasGroup cgroup;
    float timer;

    void Awake()
    {
        cgroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        cgroup.alpha = 1f;
        timer = fadeTime;
        StartCoroutine(DelayFade());
    }

    private IEnumerator DelayFade()
    {
        yield return new WaitForSeconds(fadeDelay);
        yield return Fade();
    }
    private IEnumerator Fade()
    {
        while (cgroup.alpha > 0)
        {
            timer -= Time.deltaTime;
            cgroup.alpha = Mathf.Clamp(cgroup.alpha, 0, timer / fadeTime);
            yield return null;
        }
    }
}
