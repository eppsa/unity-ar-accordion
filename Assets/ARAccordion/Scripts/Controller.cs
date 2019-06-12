using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.XR.ARFoundation;
using Newtonsoft.Json;
using Model;
using System.IO;
using System;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;

public class Controller : MonoBehaviour
{
    [SerializeField] ARTrackedImageManager trackedImageManager;
    [SerializeField] private Camera arCamera;
  
    [SerializeField] private Transform accordionPrefab;

    [SerializeField] private DebugView debugView;
    [SerializeField] private GameObject development;

    [SerializeField] private GameObject toggleButton;

    private Accordion accordion;

    private ARTrackedImage arTrackedImage;

    private int maxSteps;
    private int step;

    private Content content;

    private bool quizActive;

    void OnEnable() {
        maxSteps = accordionPrefab.Find("Content").childCount;

        ReadJson();

        if (Application.isEditor) {
            development.SetActive(true);
            accordion = development.GetComponentInChildren<Accordion>();
            accordion.SetContent(this.content);

            Camera.main.GetComponentInChildren<PostFX>().UpdateAperature(20.0f);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocalLength(150.0f);
        } else {
            development.SetActive(false);
            accordion = null;
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

            Camera.main.GetComponentInChildren<PostFX>().UpdateAperature(20.0f);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocalLength(50.0f);
        }
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            AddTrackedImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateTrackedImage(trackedImage);
        }
    }

    private void AddTrackedImage(ARTrackedImage trackedImage)
    {
        accordion = trackedImage.GetComponentInChildren<Accordion>();
        accordion.SetContent(this.content);
    }

    private void UpdateTrackedImage(ARTrackedImage trackedImage)
    {
        // Set canvas camera
        var canvas = trackedImage.transform.Find("Canvas").GetComponent<Canvas>();
        canvas.worldCamera = arCamera;

        // Update information about the tracked image
        var text = canvas.GetComponentInChildren<Text>();
        text.text = string.Format(
            "{0}\ntrackingState: {1}\nGUID: {2}\nReference size: {3} cm\nDetected size: {4} cm\nCamera Position: {5}\nTracked Image position: {6}",
            trackedImage.referenceImage.name,
            trackedImage.trackingState,
            trackedImage.referenceImage.guid,
            trackedImage.referenceImage.size * 100f,
            trackedImage.size * 100f, // 0.249 * 100
            arCamera.transform.position,
            trackedImage.transform.position);

        var planeParentGo = trackedImage.transform.Find("PlaneParent").gameObject;
        var planeGo = planeParentGo.transform.GetChild(0).gameObject;

        if (trackedImage.trackingState != TrackingState.None)
        {
            planeGo.SetActive(true);

            trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 0.1f, trackedImage.size.y);

            var material = planeGo.GetComponentInChildren<MeshRenderer>().material;
            material.mainTexture = trackedImage.referenceImage.texture;
        }
        else
        {
            planeGo.SetActive(false);
        }
    }

    void ReadJson()
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "content.json");
        string jsonString = File.ReadAllText(jsonPath);
        //content = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonString);
        content = JsonConvert.DeserializeObject<Content>(jsonString);
    }

    void Update()
    {
        if (!quizActive) {
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

            if (Input.touchCount > 0) {
                Touch touch = Input.GetTouch(0);

                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return;
                }

                if (touch.phase == TouchPhase.Began) {
                    if (touch.position.x < 1000) {
                        if (step > 0) { 
                            step--;
                        }
                    } else {
                        if (step < maxSteps) { 
                            step++;
                        }
                    }
                    accordion.UpdateStep(step);
                    debugView.Refresh(step);
                }   
            }

            toggleButton.SetActive(step > 0);
        }
    }

    public void OnActivateTowardsCamera(bool active) {
        if (accordion) {
            accordion.SetMoveTowardsCamera(active);
        }
    }

    public void OnShowReferenceImage(bool show) {
    }

    public void OnToggleQuiz() {
        quizActive = !quizActive;
        accordion.ShowQuiz(quizActive);

        toggleButton.GetComponentInChildren<Text>().text = quizActive ? "Accordion" : "Quiz";
    }

    public void OnEnableDofDebugging(bool enable) {
       arCamera.GetComponent<PostProcessDebug>().enabled = enable;
       if (accordion != null){
           accordion.transform.Find("Plane").gameObject.SetActive(enable);
       }
    }
}
