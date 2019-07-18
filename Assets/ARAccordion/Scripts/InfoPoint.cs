using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InfoPoint : Button
{
    InfoTag infoTag;

    private string content;
    private string imagePath;
    private float delay;

    private bool scaleRunning = false;

    private float duration = 0.7f;

    protected override void OnEnable()
    {
        infoTag = GetComponentInChildren<InfoTag>();
        infoTag.gameObject.SetActive(false);

        GetComponentInChildren<Canvas>().worldCamera = Camera.main;

        transform.localScale = Vector3.zero;

        Invoke("StartScaling", this.delay);
    }

    private void StartScaling()
    {
        StartCoroutine(Scale(0.0f, 1.0f, duration));
    }

    private IEnumerator Scale(float from, float to, float duration)
    {
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
                yield break;
            }
        }
    }

    internal void SetDelay(float delay)
    {
        this.delay = delay;
    }

    internal void SetContent(string content)
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

    internal void ShowInfoTag(TagAnchor.Orientation orientation)
    {
        float width = infoTag.transform.Find("Background").GetComponent<RectTransform>().sizeDelta.x + transform.Find("Canvas").GetComponent<RectTransform>().sizeDelta.x * 0.5f;

        if (orientation == TagAnchor.Orientation.LEFT) {
            infoTag.transform.localPosition = new Vector3(-width * infoTag.transform.localScale.x, infoTag.transform.localPosition.y, infoTag.transform.localPosition.z);
        }

        infoTag.gameObject.SetActive(true);
        infoTag.Show(content, imagePath);
    }
}