using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.XR.ARFoundation;
using Newtonsoft.Json;
using System.IO;

public class Controller : MonoBehaviour
{
    [SerializeField] private ARSessionOrigin sessionOrigin;
    [SerializeField] private Camera arCamera;
    [SerializeField] private PostFX postFx;
  
    [SerializeField] private Transform accordionPrefab;

    [SerializeField] private DebugView debugView;

    [SerializeField] private GameObject development;

    private Accordion accordion;

    private ARTrackedImage arTrackedImage;

    private int maxSteps;
    private int step;

    private Dictionary<string, Dictionary<string, string>> content;

    void OnEnable() {
        ReadJson();

        if (Application.isEditor) {
            development.SetActive(true);
            accordion = development.GetComponentInChildren<Accordion>();
        } else {
            development.SetActive(false);
            accordion = null;
        }
    }

    void ReadJson()
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "content.json");
        string jsonString = File.ReadAllText(jsonPath);
        content = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonString);
    }

    void Start() {
        maxSteps = accordionPrefab.Find("Content").childCount;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetAxis("Mouse ScrollWheel") < 0) {
            if (step > 0) {
                step--;
                accordion.UpdateStep(step);
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetAxis("Mouse ScrollWheel") > 0) {
            if (step < maxSteps) { 
                step++;
                accordion.UpdateStep(step);
            }
        }

        // if (Input.touchCount > 0) {
        //     Touch touch = Input.GetTouch(0);

        //     if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        //     {
        //         return;
        //     }

        //     if (touch.phase == TouchPhase.Began) {
        //         if (touch.position.x < 1000) {
        //             if (step > 0) { 
        //                 step--;
        //             }
        //         } else {
        //             if (step < maxSteps) { 
        //                 step++;
        //             }
        //         }
        //         accordion.UpdateStep(step);
        //         debugView.Refresh(step);
        //     }   
        // }

        if (accordion == null) {
            Transform arTrackedImageTransform = sessionOrigin.trackablesParent.childCount != 0 ? sessionOrigin.trackablesParent.GetChild(0) : null;
            if (arTrackedImageTransform != null) {
                arTrackedImage = arTrackedImageTransform.GetComponent<ARTrackedImage>();
                accordion = arTrackedImage.GetComponentInChildren<Accordion>();
                accordion.SetContent(this.content);
                accordion.SetSessionOrigin(sessionOrigin);
            };
        } else {
            accordion.SetContent(this.content);
        }
    }

    public void OnActivateTowardsCamera(bool active) {
       accordion.SetMoveTowardsCamera(active);
    }

    public void OnShowReferenceImage(bool show) {
       accordion.ShowReferenceImage(show);
    }
}
