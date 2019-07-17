using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InfoPoint : Button
{
    InfoTag infoTag;

    string content;
    string imagePath;

    protected override void OnEnable()
    {
        Debug.Log("OnEnalbe()");

        infoTag = GetComponentInChildren<InfoTag>();
        infoTag.gameObject.SetActive(false);

        GetComponentInChildren<Canvas>().worldCamera = Camera.main;

        // 1. Play Appear Animation
    }

    internal void SetContent(string content)
    {
        this.content = content;
    }

    internal void SetImagePath(string imagePath)
    {
        this.imagePath = imagePath;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick()");

        // 1. Play Click Aninmation

        // 2. Hide opened info tag

        // 3. Show info tag and set content
        infoTag.gameObject.SetActive(true);
        infoTag.Show(content, imagePath);
    }
}