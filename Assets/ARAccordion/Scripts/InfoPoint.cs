using System.Collections;
using Model;
using UnityEngine;
using UnityEngine.UI;

public class InfoPoint : Button
{
    InfoTag infoTag;

    private Info content;
    private string imagePath;
    private float delay;

    private bool scaleRunning = false;

    private const float FADE_IN_DURATION = 0.35f;
    private const float FADE_OUT_DURATION = 0.2f;

    protected override void OnEnable()
    {
        infoTag = GetComponentInChildren<InfoTag>();
        infoTag.gameObject.SetActive(false);

        transform.localScale = Vector3.zero;

        Invoke("StartScaling", this.delay);
    }

    private void StartScaling()
    {
        StartCoroutine(Scale(0.0f, 1.0f, FADE_IN_DURATION));
    }

    private IEnumerator Scale(float from, float to, float duration)
    {
        scaleRunning = true;

        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true) {
            currentDuration = Time.time - startTime;
            progress = currentDuration / duration;

            if (progress <= 1.0f) {
                float value = Mathf.Lerp(from, to, progress);
                transform.localScale = new Vector3(value, value, 1);
                yield return new WaitForEndOfFrame();
            } else {
                transform.localScale = new Vector3(to, to, 1);
                scaleRunning = false;
                yield break;
            }
        }
    }

    internal void SetDelay(float delay)
    {
        this.delay = delay;
    }

    internal void SetContent(Info content)
    {
        this.content = content;
    }

    internal void SetImagePath(string imagePath)
    {
        this.imagePath = imagePath;
    }

    internal void HideInfoTag()
    {
        infoTag.Hide();
    }

    internal void Hide()
    {
        CancelInvoke();
        StopAllCoroutines();
        StartCoroutine(Scale(transform.localScale.x, 0.0f, FADE_OUT_DURATION));
        StartCoroutine(Delete());
    }

    IEnumerator Delete()
    {
        while (scaleRunning) {
            yield return null;
        }

        gameObject.SetActive(false);
    }

    internal void ShowInfoTag(TagAnchor.Orientation orientation)
    {
        float offset = infoTag.transform.Find("Background").GetComponent<RectTransform>().sizeDelta.x + transform.Find("Canvas").GetComponent<RectTransform>().sizeDelta.x * 0.5f;

        if (orientation == TagAnchor.Orientation.LEFT) {
            infoTag.transform.localPosition = new Vector3(-offset * infoTag.transform.localScale.x, infoTag.transform.localPosition.y, infoTag.transform.localPosition.z);
        }

        infoTag.gameObject.SetActive(true);
        infoTag.Show(content, imagePath);
    }

    protected override void OnDisable()
    {
        Destroy(this.gameObject);
    }
}