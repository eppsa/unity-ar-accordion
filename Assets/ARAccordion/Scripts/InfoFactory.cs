using System;
using System.Collections;
using System.Collections.Generic;
using Model;
using UnityEngine;

public class InfoFactory : MonoBehaviour
{
    private const float FADE_IN_DELAY = 0.35f;
    [SerializeField] GameObject infoPointPrefab;
    [SerializeField] GameObject infoTagPrefab;

    private AudioSource clickSound;

    private InfoPoint selectedInfoPoint;

    void OnEnable()
    {
        infoPointPrefab.SetActive(false);
        infoTagPrefab.SetActive(false);

        clickSound = GameObject.Find("Sounds/Click").GetComponent<AudioSource>();
    }

    public void CreateInfoPoints(List<Info> infos, Transform anchors)
    {
        for (int i = 0; i < infos.Count; i++) {
            InfoPoint infoPoint = Instantiate(infoPointPrefab).GetComponent<InfoPoint>();
            infoPoint.transform.SetParent(anchors.GetChild(i), false);

            string imagePath = "Avatars/" + infos[i].image;
            imagePath = imagePath.Replace("Layer", "");

            infoPoint.SetContent(infos[i]);
            infoPoint.SetImagePath(imagePath);
            infoPoint.SetDelay(i * FADE_IN_DELAY);

            infoPoint.gameObject.SetActive(true);

            infoPoint.onClick.AddListener(() => OnInfoPointClick(infoPoint));
        }
    }

    public void ClearInfoPoints(Transform anchors)
    {
        foreach (Transform anchor in anchors) {
            if (anchor.childCount > 0) {
                InfoPoint infoPoint = anchor.GetComponentInChildren<InfoPoint>();
                if (infoPoint != null) {
                    infoPoint.Hide();
                }

                Transform extraImages = anchor.Find("ExtraImages");
                if (extraImages != null) {
                    HideExtraImages(extraImages);
                }
            }
        }
    }

    public void CreateInfoTag(Info content, Transform anchor)
    {
        InfoTag infoTag = Instantiate(infoTagPrefab).GetComponent<InfoTag>();
        infoTag.transform.SetParent(anchor, false);

        infoTag.gameObject.SetActive(true);

        infoTag.Show(content);
    }

    internal void ClearInfoTag(Transform anchor)
    {
        InfoTag infoTag = anchor.GetComponentInChildren<InfoTag>();

        if (infoTag != null) {
            infoTag.Hide();
        }
    }

    private void OnInfoPointClick(InfoPoint infoPoint)
    {
        clickSound.Play();

        if (selectedInfoPoint != null) {
            selectedInfoPoint.HideInfoTag();
            selectedInfoPoint.interactable = true;

            Transform extraImages = this.selectedInfoPoint.transform.parent.Find("ExtraImages");
            if (extraImages) {
                HideExtraImages(extraImages);
            }
        }

        infoPoint.interactable = false;
        this.selectedInfoPoint = infoPoint;

        ShowInfoTag();
        ShowExtraImages();
    }

    private void ShowInfoTag()
    {
        TagAnchor.Orientation orientation = this.selectedInfoPoint.transform.GetComponentInParent<TagAnchor>().orientation;

        this.selectedInfoPoint.ShowInfoTag(orientation);
    }

    private void ShowExtraImages()
    {
        Transform extraImages = this.selectedInfoPoint.transform.parent.Find("ExtraImages");

        if (extraImages) {
            StartCoroutine(DoFade(extraImages, 1, 1));
        }
    }

    private void HideExtraImages(Transform extraImages)
    {
        StartCoroutine(DoFade(extraImages, 0, 1));
    }

    private IEnumerator DoFade(Transform extraImages, float to, float duration)
    {
        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true) {
            currentDuration = Time.time - startTime;
            progress = currentDuration / duration;

            if (progress <= 1.0f) {
                Color color = extraImages.GetComponentInChildren<SpriteRenderer>().color;

                float alpha = Mathf.Lerp(color.a, to, progress);

                color = new Color(color.r, color.g, color.b, alpha);
                extraImages.GetComponentInChildren<SpriteRenderer>().color = color;

                yield return new WaitForEndOfFrame();
            } else {
                Color color = extraImages.GetComponentInChildren<SpriteRenderer>().color;
                color = new Color(color.r, color.g, color.b, to);
                extraImages.GetComponentInChildren<SpriteRenderer>().color = color;

                yield break;
            }
        }
    }
}
