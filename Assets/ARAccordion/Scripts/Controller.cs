using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.XR.ARFoundation;

public class Controller : MonoBehaviour
{
    [SerializeField] private ARSessionOrigin sessionOrigin;
    [SerializeField] private Camera arCamera;
    [SerializeField] private PostFX postFx;
    [SerializeField] private InfoPopup infoPopUp;

    [SerializeField] private Transform accordionPrefab;

    private Accordion accordion;
    private ARTrackedImage arTrackedImage;

    private int maxSteps;
    private int step;

    void Start() {
        maxSteps = accordionPrefab.Find("Content").childCount;
        Debug.Log("Max steps: " + maxSteps);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetAxis("Mouse ScrollWheel") < 0) {
            if (step > 0) {
                step--;
                infoPopUp.SwitchLayer(step);
                accordion.UpdateStep(step);
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetAxis("Mouse ScrollWheel") > 0) {
            if (step < maxSteps) { 
                step++;
                infoPopUp.SwitchLayer(step);
                accordion.UpdateStep(step);
            }
        }

        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            Debug.Log(touch.position);

            if (touch.phase == TouchPhase.Ended) {
                if (touch.position.x < 1000) {
                    if (step > 0) { 
                        step--;
                    }
                } else {
                    if (step < maxSteps) { 
                        step++;
                    }
                }
        
                infoPopUp.GetComponent<InfoPopup>().SwitchLayer(step);
                accordion.UpdateStep(step);
            }   
        }

        if (accordion == null) {
            Transform arTrackedImageTransform = sessionOrigin.trackablesParent.childCount != 0 ? sessionOrigin.trackablesParent.GetChild(0) : null;
            if (arTrackedImageTransform != null) {
                arTrackedImage = arTrackedImageTransform.GetComponent<ARTrackedImage>();
                accordion = arTrackedImage.GetComponentInChildren<Accordion>();
            };
        } else {
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

            // infoPopUp.GetComponent<InfoPopup>().SwitchLayer(step);
        }
    }
}
