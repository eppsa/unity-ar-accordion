using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class StartScreen : MonoBehaviour
{
    private bool isFadeing;
    private float duration = 0.7f;

    private AudioSource clickSound;

    public UnityEvent onStart;

    void OnEnable()
    {
        clickSound = GameObject.Find("Sounds/Click").GetComponent<AudioSource>();
    }

    public void Show(bool show)
    {
        StartCoroutine(DoShow(show));
    }

    private IEnumerator DoShow(bool show)
    {
        if (show) {
            StartCoroutine(Fade(0.0f, 1.0f, duration));
        } else {
            StartCoroutine(Fade(1.0f, 0.0f, duration));
            while (isFadeing) {
                yield return null;
            }

            this.transform.gameObject.SetActive(false);
            onStart.Invoke();
        }
    }

    private IEnumerator Fade(float fadeFrom, float fadeTo, float duration)
    {
        isFadeing = true;

        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true) {
            currentDuration = Time.time - startTime;
            progress = currentDuration / duration;

            if (progress <= 1.0f) {
                GetComponent<CanvasGroup>().alpha = Mathf.Lerp(fadeFrom, fadeTo, progress);
                yield return new WaitForEndOfFrame();
            } else {
                this.GetComponent<CanvasGroup>().alpha = fadeTo;
                isFadeing = false;
                yield break;
            }
        }
    }

    public void OnStartButton()
    {
        if (!isFadeing) {
            clickSound.Play();
            StartCoroutine(DoShow(false));
        }
    }
}
