using System.Collections.Generic;
using UnityEngine;

public class InfoFactory : MonoBehaviour
{
    [SerializeField] GameObject infoPointPrefab;

    private InfoPoint selectedInfoPoint;

    void OnEnable()
    {
        infoPointPrefab.SetActive(false);
    }

    public void Create(List<string> infos, Transform anchors, string imagePath)
    {
        for (int i = 0; i < infos.Count; i++) {
            InfoPoint infoPoint = Instantiate(infoPointPrefab).GetComponent<InfoPoint>();
            infoPoint.transform.SetParent(anchors.GetChild(i), false);

            infoPoint.SetContent(infos[i]);
            infoPoint.SetImagePath(imagePath);
            infoPoint.SetDelay(i * 0.7f);

            infoPoint.gameObject.SetActive(true);

            infoPoint.onClick.AddListener(() => OnInfoPointClick(infoPoint));
        }
    }

    public void Clear(Transform anchors)
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

    private void OnInfoPointClick(InfoPoint infoPoint)
    {
        Debug.Log("OnPointerClick()");

        // 1. Play click aninmation

        if (selectedInfoPoint != null) {
            selectedInfoPoint.HideInfoTag();
        }

        this.selectedInfoPoint = infoPoint;

        TagAnchor.Orientation orientation = infoPoint.transform.GetComponentInParent<TagAnchor>().orientation;

        this.selectedInfoPoint.ShowInfoTag(orientation);
    }

}
