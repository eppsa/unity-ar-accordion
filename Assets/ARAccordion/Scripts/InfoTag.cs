using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InfoTag : MonoBehaviour
{
    private float duration = 0.7f;

    [SerializeField] private GameObject image;
    [SerializeField] private Text text;

    string content;

    private string imagePath;

    bool fadeRunning = false;

    void OnEnable()
    {
        GetComponent<CanvasGroup>().alpha = 0.0f;
    }

    public void Show(string content, string imagePath)
    {
        this.content = content;
        this.imagePath = imagePath;

        StartCoroutine(DoShow());
        DoShow();
    }

    private IEnumerator DoShow()
    {
        while (fadeRunning) {
            yield return null;
        }

        UpdateInformation();
        StartCoroutine(Fade(0.0f, 1.0f, duration));
    }

    public void Hide()
    {
        if (this.GetComponent<CanvasGroup>().alpha == 0.0f) {
            return;
        }

        StartCoroutine(Fade(1.0f, 0.0f, duration));
    }

    private IEnumerator Fade(float fadeFrom, float fadeTo, float duration)
    {
        fadeRunning = true;

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
                fadeRunning = false;
                yield break;
            }
        }
    }

    private void UpdateInformation()
    {
        text.text = content;
        image.GetComponent<Image>().sprite = Resources.Load<Sprite>(imagePath);
    }

    public void SetFadeDuration(float duration)
    {
        this.duration = duration;
    }

    void OnDisable()
    {
        fadeRunning = false;
    }
}
