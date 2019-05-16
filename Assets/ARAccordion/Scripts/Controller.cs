using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.XR.ARFoundation;

public class Controller : MonoBehaviour
{
    [SerializeField] ARSessionOrigin sessionOrigin;
    [SerializeField] Camera arCamera;
    [SerializeField] PostFX postFx;

    private Accordion accordion;
    private ARTrackedImage arTrackedImage;

    void Update()
    {
        if (accordion == null) {
            Transform arTrackedImageTransform = sessionOrigin.trackablesParent.childCount != 0 ? sessionOrigin.trackablesParent.GetChild(0) : null;
            if (arTrackedImageTransform != null) {
                arTrackedImage = arTrackedImageTransform.GetComponent<ARTrackedImage>();
                accordion = arTrackedImage.GetComponentInChildren<Accordion>();
            };
        } else {
            int step = accordion.step;

            Debug.Log("Size: " + arTrackedImage.size);
            Debug.Log("Position: " + arTrackedImage.transform.position);

            Transform content = accordion.transform.Find("Content");

            if (content != null) {
                Debug.Log("Child count: " + content.childCount);

                Transform layer = content.GetChild(content.childCount - step - 1); 
                Debug.Log("Layer position: " + layer.position);
                Debug.Log("Layer local position: " + layer.localPosition);


                float distance = Vector3.Distance(arCamera.transform.localPosition, layer.position);
                Debug.Log("Distance: " + distance);

                // postFx.UpdateFocusDistance(distance);
            }
        }
    }
}
