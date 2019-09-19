using System.Collections;
using Model;
using UnityEngine;
using UnityEngine.UI;

public class InfoTag : MonoBehaviour
{
    private const float WRITE_LETTER_DELAY = 0.02f;
    private float duration = 0.7f;

    [SerializeField] private GameObject background;
    [SerializeField] private GameObject image;
    [SerializeField] private Text text;

    Info content;

    private string imagePath;

    bool scaleRunning = false;

    void OnEnable()
    {
        background.transform.localScale = new Vector3(0, 1, 1);
        image.transform.localScale = new Vector3(0, 0, 0);

        text.gameObject.SetActive(false);
    }

    public void Show(Info content)
    {
        Show(content, null);
    }

    public void Show(Info content, string imagePath)
    {
        this.content = content;
        this.imagePath = imagePath;

        ShowBackground();

        if (imagePath != null) {
            StartCoroutine(ShowImage());
        }

        StartCoroutine(WriteText());
    }

    private void ShowBackground()
    {
        UpdateContent();
        StartCoroutine(DoScale(background.transform, background.transform.localScale, Vector3.one, duration));
    }

    private IEnumerator ShowImage()
    {
        while (scaleRunning) {
            yield return null;
        }

        StartCoroutine(DoScale(image.transform, image.transform.localScale, Vector3.one, duration));
    }

    private IEnumerator DoScale(Transform transform, Vector3 from, Vector3 to, float duration)
    {
        scaleRunning = true;

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
                scaleRunning = false;
                yield break;
            }
        }
    }

    private IEnumerator WriteText()
    {
        while (scaleRunning) {
            yield return null;
        }

        StartCoroutine(DoWriteText(WRITE_LETTER_DELAY));
    }

    private IEnumerator DoWriteText(float letterDelay)
    {
        string fullText = text.text;

        text.text = "";

        text.gameObject.SetActive(true);
        text.transform.localScale = Vector3.one;

        for (int i = 0; i <= fullText.Length; i++) {
            text.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(letterDelay);
        }
    }

    private void UpdateContent()
    {
        text.text = content.text;
        image.GetComponent<Image>().sprite = Resources.Load<Sprite>(imagePath);
    }

    public void SetFadeDuration(float duration)
    {
        this.duration = duration;
    }

    public void Hide()
    {
        StopAllCoroutines();
        scaleRunning = false;

        StartCoroutine(DoHide());
        StartCoroutine(Delete());
    }

    IEnumerator DoHide()
    {
        while (scaleRunning) {
            yield return null;
        }

        StartCoroutine(DoScale(background.transform, background.transform.localScale, new Vector3(0, 1, 1), duration));
        StartCoroutine(DoScale(image.transform, image.transform.localScale, Vector3.zero, 0.2f));
        StartCoroutine(DoScale(text.transform, text.transform.localScale, Vector3.zero, 0.2f));
    }

    IEnumerator Delete()
    {
        while (scaleRunning) {
            yield return null;
        }

        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        scaleRunning = false;
    }
}
