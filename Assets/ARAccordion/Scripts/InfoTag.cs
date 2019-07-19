using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InfoTag : MonoBehaviour
{
    private float duration = 0.7f;

    [SerializeField] private GameObject background;
    [SerializeField] private GameObject image;
    [SerializeField] private Text text;

    string content;

    private string imagePath;

    bool backgroundScaleRunning = false;
    bool imageScaleRunning = false;

    void OnEnable()
    {
        background.transform.localScale = new Vector3(0, 1, 1);
        image.transform.localScale = new Vector3(0, 0, 0);

        text.gameObject.SetActive(false);
    }

    public void Show(string content, string imagePath)
    {
        this.content = content;
        this.imagePath = imagePath;

        ShowBackground();

        if (image) {
            StartCoroutine(ShowImage());
        }

        StartCoroutine(WriteText());
    }

    private void ShowBackground()
    {
        UpdateInformation();
        StartCoroutine(DoScale(background.transform, background.transform.localScale, Vector3.one, duration));
    }

    private IEnumerator ShowImage()
    {
        while (backgroundScaleRunning) {
            yield return null;
        }

        StartCoroutine(DoScale(image.transform, image.transform.localScale, Vector3.one, duration));
    }

    private IEnumerator DoScale(Transform transform, Vector3 from, Vector3 to, float duration)
    {
        backgroundScaleRunning = true;

        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true) {
            currentDuration = Time.time - startTime;
            progress = currentDuration / duration;

            if (progress <= 1.0f) {
                transform.localScale = Vector3.Lerp(from, to, progress);
                yield return new WaitForEndOfFrame();
            } else {
                transform.localScale = to;
                backgroundScaleRunning = false;
                yield break;
            }
        }
    }

    private IEnumerator WriteText()
    {
        while (backgroundScaleRunning || imageScaleRunning) {
            yield return null;
        }

        StartCoroutine(DoWriteText(0.02f));
    }

    private IEnumerator DoWriteText(float letterDelay)
    {
        string fullText = text.text;

        text.text = "";

        text.gameObject.SetActive(true);
        text.transform.localScale = Vector3.one;

        for (int i = 0; i < fullText.Length; i++) {
            text.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(letterDelay);
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
        StopAllCoroutines();

        StartCoroutine(DoScale(background.transform, background.transform.localScale, new Vector3(0, 1, 1), duration));
        StartCoroutine(DoScale(image.transform, image.transform.localScale, Vector3.zero, 0.2f));
        StartCoroutine(DoScale(text.transform, text.transform.localScale, Vector3.zero, 0.2f));
    }

    void OnDisable()
    {
        backgroundScaleRunning = false;
    }
}
