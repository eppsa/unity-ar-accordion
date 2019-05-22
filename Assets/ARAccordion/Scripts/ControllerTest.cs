using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.XR.ARFoundation;

public class ControllerTest : MonoBehaviour
{
    [SerializeField] private ARSessionOrigin sessionOrigin;
    [SerializeField] private Camera arCamera;
    [SerializeField] private PostFX postFx;
    [SerializeField] private InfoPopup infoPopUp;

    [SerializeField] private Transform accordionPrefab;

    [SerializeField] private Transform canvasPrefab;

    private Accordion accordion;
    private ARTrackedImage arTrackedImage;

    private int maxSteps;
    private int step;

    void Start() {
        maxSteps = accordionPrefab.Find("Content").childCount;
        accordionPrefab.GetComponent<Accordion>().SetTargetPosition(arCamera.transform);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetAxis("Mouse ScrollWheel") < 0) {
            if (step > 0) {
                step--;
                infoPopUp.SwitchLayer(step);
                accordionPrefab.GetComponent<Accordion>().UpdateStep(step);
                canvasPrefab.transform.position = accordionPrefab.GetComponent<Accordion>().getActiveTilePosition();

                // Update only z value of canvas
                //
                // canvasPrefab.transform.position = new Vector3 ( 
                //     canvasPrefab.transform.position.x,
                //     canvasPrefab.transform.position.y,
                //     accordionPrefab.GetComponent<Accordion>().activeTilePosition.z
                // );
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetAxis("Mouse ScrollWheel") > 0) {
            if (step < maxSteps) { 
                step++;
                infoPopUp.SwitchLayer(step);
                accordionPrefab.GetComponent<Accordion>().UpdateStep(step);
                canvasPrefab.transform.position = accordionPrefab.GetComponent<Accordion>().getActiveTilePosition();


            }
        }

        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);

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
                accordionPrefab.GetComponent<Accordion>().UpdateStep(step);
                canvasPrefab.transform.position = accordionPrefab.GetComponent<Accordion>().getActiveTilePosition();
            }   
        }

        // if (accordion == null) {
        //     Transform arTrackedImageTransform = sessionOrigin.trackablesParent.childCount != 0 ? sessionOrigin.trackablesParent.GetChild(0) : null;
        //     if (arTrackedImageTransform != null) {
        //         arTrackedImage = arTrackedImageTransform.GetComponent<ARTrackedImage>();
        //         accordion = arTrackedImage.GetComponentInChildren<Accordion>();
        //     };
        // } else {
        //     accordion.SetTargetPosition(arCamera.transform);

        //     Transform content = accordion.transform.Find("Content");

        //     if (content != null) {
        //         Transform layer = content.GetChild(content.childCount - step - 1); 
        //         float distance = Vector3.Distance(arCamera.transform.localPosition, layer.position);

        //         // postFx.UpdateFocusDistance(distance);
        //     }

        //     // infoPopUp.GetComponent<InfoPopup>().SwitchLayer(step);
        // }
    }

    public void OnAccodrionScaleFactorZChange(float factor) {
        if (accordion != null) {
            // accordion.SetScaleFactorZ(factor);
        }
    }
}