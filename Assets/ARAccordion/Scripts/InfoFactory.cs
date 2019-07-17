using System.Collections.Generic;
using UnityEngine;

public class InfoFactory : MonoBehaviour
{
    [SerializeField] GameObject infoPointPrefab;

    public void Create(List<string> infos, Transform anchors, string imagePath)
    {
        for (int i = 0; i < infos.Count; i++) {
            InfoPoint infoPoint = Instantiate(infoPointPrefab).GetComponent<InfoPoint>();
            infoPoint.transform.SetParent(anchors.GetChild(i), false);

            infoPoint.SetContent(infos[i]);

            infoPoint.SetImagePath(imagePath);
        }
    }

    public void Clear(Transform anchors)
    {
        Debug.Log("Clear");

        foreach (Transform anchor in anchors) {
            GameObject infoPoint = anchor.GetChild(0).gameObject;
            infoPoint.SetActive(false);

            anchor.DetachChildren();

            Destroy(infoPoint);
            Debug.Log("Cleared " + infoPoint);
        }
    }
}
