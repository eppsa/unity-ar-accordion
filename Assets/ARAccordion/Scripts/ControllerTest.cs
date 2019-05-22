using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.XR.ARFoundation;

public class ControllerTest : MonoBehaviour
{
    [SerializeField] private ARSessionOrigin sessionOrigin;
    [SerializeField] private Camera arCamera;
    [SerializeField] private PostFX postFx;
    [SerializeField] private InfoPopup infoPopUp;

    [SerializeField] private Transform accordionPrefab;

    private GameObject accordion;
    private ARTrackedImage arTrackedImage;

    private int maxSteps;
    private int step;

    void Start() {
        maxSteps = accordionPrefab.Find("Content").childCount;
        Debug.Log("Max steps: " + maxSteps);
        accordion = accordionPrefab.gameObject;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetAxis("Mouse ScrollWheel") < 0) {
            if (step > 0) {
                step--;
                infoPopUp.SwitchLayer(step);
                accordion.GetComponent<Accordion>().UpdateStep(step);
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetAxis("Mouse ScrollWheel") > 0) {
            if (step < maxSteps) { 
                step++;
                infoPopUp.SwitchLayer(step);
                accordion.GetComponent<Accordion>().UpdateStep(step);

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
                accordion.GetComponent<Accordion>().UpdateStep(step);

            }   
        }

       
    }

    public void OnAccodrionScaleFactorZChange(float factor) {
        if (accordion != null) {
            accordion.GetComponent<Accordion>().SetScaleFactorZ(factor);
        }
    }
}
