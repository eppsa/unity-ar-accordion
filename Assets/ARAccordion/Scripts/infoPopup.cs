using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InfoPopup : MonoBehaviour
{
    private float duration = 0.7f;
    
    [SerializeField] private GameObject image;
    [SerializeField] private Text text;

    Dictionary<string, Dictionary<string, string>> content;

    private int layer = 0;
    private Transform anchor;

    bool fadeRunning = false;


    public void Show(int layer)
    {
        this.layer = layer;

        StartCoroutine(DoShow());
    }

    private IEnumerator DoShow() {
        // Hide();

        while (fadeRunning) {
            yield return null;
        }

        UpdateInformation();
        StartCoroutine(Fade(0.0f, 1.0f, duration));
    }

    public void Hide() {
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

        while (true)
        {
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
        transform.position = this.anchor.transform.position;
        transform.parent = this.anchor.transform;

        text.text = content["layer" + layer]["information"];
        image.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/icon" + layer);
    }

    public void SetContent(Dictionary<string, Dictionary<string, string>> content) {
        this.content = content;
    }

    public void SetAnchor(Transform anchor) {
        this.anchor = anchor;
    }
    
    public void SetFadeDuration(float duration) {
        this.duration = duration;
    }
}