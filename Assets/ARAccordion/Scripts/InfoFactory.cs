using System.Collections.Generic;
using UnityEngine;

public class InfoFactory : MonoBehaviour
{
    private const float FADE_IN_DELAY = 0.7f;
    [SerializeField] GameObject infoPointPrefab;
    [SerializeField] GameObject infoTagPrefab;

    private InfoPoint selectedInfoPoint;

    void OnEnable()
    {
        infoPointPrefab.SetActive(false);
        infoTagPrefab.SetActive(false);
    }

    public void CreateInfoPoints(List<string> infos, Transform anchors, string imagePath)
    {
        for (int i = 0; i < infos.Count; i++) {
            InfoPoint infoPoint = Instantiate(infoPointPrefab).GetComponent<InfoPoint>();
            infoPoint.transform.SetParent(anchors.GetChild(i), false);

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
                InfoPoint infoPoint = anchor.GetChild(0).gameObject.GetComponent<InfoPoint>();

                if (infoPoint != null) {
                    infoPoint.Hide();
                }
            }
        }
    }

    public void CreateInfoTag(string content, Transform anchor)
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
        if (selectedInfoPoint != null) {
            selectedInfoPoint.HideInfoTag();
        }

        this.selectedInfoPoint = infoPoint;

        TagAnchor.Orientation orientation = infoPoint.transform.GetComponentInParent<TagAnchor>().orientation;

        this.selectedInfoPoint.ShowInfoTag(orientation);
    }
}
