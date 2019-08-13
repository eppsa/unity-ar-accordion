using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
    [SerializeField] Button startButton;

    private bool isFadeing;
    private float duration = 0.7f;

    public UnityEvent onStart;


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

            startButton.interactable = false;

            while (isFadeing) {
                yield return null;
            }

            startButton.interactable = true;
            this.transform.gameObject.SetActive(false);
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
}
