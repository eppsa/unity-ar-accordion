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

    bool backgroundScaleRunning = false;
    bool imageScaleRunning = false;

    void OnEnable()
    {
        transform.Find("Background").localScale = new Vector3(0, 1, 1);
        transform.Find("Image").localScale = new Vector3(0, 0, 0);
        transform.Find("Text").GetComponent<Text>().color = new Color(0, 0, 0, 0);
    }

    public void Show(string content, string imagePath)
    {
        this.content = content;
        this.imagePath = imagePath;

        DoShowBackground();
        StartCoroutine(DoShowImage());
        StartCoroutine(DoShowText());
    }

    private void DoShowBackground()
    {
        UpdateInformation();
        StartCoroutine(ScaleBackground(0.0f, 1.0f, duration));
    }

    private IEnumerator DoShowImage()
    {
        while (backgroundScaleRunning) {
            yield return null;
        }

        StartCoroutine(ScaleImage(0.0f, 1.0f, duration));
    }

    private IEnumerator DoShowText()
    {
        while (backgroundScaleRunning || imageScaleRunning) {
            yield return null;
        }

        StartCoroutine(FadeText(0.0f, 1.0f, duration));
    }

    private IEnumerator ScaleBackground(float from, float to, float duration)
    {
        backgroundScaleRunning = true;

        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true) {
            currentDuration = Time.time - startTime;
            progress = currentDuration / duration;

            if (progress <= 1.0f) {
                transform.Find("Background").localScale = new Vector3(Mathf.Lerp(from, to, progress), 1, 1);
                yield return new WaitForEndOfFrame();
            } else {
                transform.Find("Background").localScale = new Vector3(to, 1, 1);
                backgroundScaleRunning = false;
                yield break;
            }
        }
    }

    private IEnumerator ScaleImage(float from, float to, float duration)
    {
        imageScaleRunning = true;

        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true) {
            currentDuration = Time.time - startTime;
            progress = currentDuration / duration;

            if (progress <= 1.0f) {
                float value = Mathf.Lerp(from, to, progress);
                transform.Find("Image").localScale = new Vector3(value, value, 1);
                yield return new WaitForEndOfFrame();
            } else {
                transform.Find("Image").localScale = new Vector3(to, to, 1);
                imageScaleRunning = false;
                yield break;
            }
        }
    }

    private IEnumerator FadeText(float from, float to, float duration)
    {
        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true) {
            currentDuration = Time.time - startTime;
            progress = currentDuration / duration;

            Text text = transform.Find("Text").GetComponent<Text>();

            if (progress <= 1.0f) {
                text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(from, to, progress));
                yield return new WaitForEndOfFrame();
            } else {
                text.color = new Color(text.color.r, text.color.g, text.color.b, to);
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

    public void Hide()
    {
        StartCoroutine(ScaleBackground(1.0f, 0.0f, duration));
        StartCoroutine(ScaleImage(1.0f, 0.0f, duration));
        StartCoroutine(FadeText(1.0f, 0.0f, duration));
    }

    void OnDisable()
    {
        backgroundScaleRunning = false;
    }
}
