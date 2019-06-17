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

    [SerializeField] private PostFX postFx;

    private Accordion accordion;

    private ARTrackedImage arTrackedImage;

    private int maxSteps;
    private int step;

    private Content content;

    private bool quizActive;
    private GameObject planeGo;
    private bool showReferenceImage = true;

    void OnEnable() {
        maxSteps = accordionPrefab.Find("Content").childCount;

        ReadJson();

        if (Application.isEditor) {
            development.SetActive(true);
            accordion = development.GetComponentInChildren<Accordion>();
            accordion.SetContent(this.content);

            postFx.UpdateAperature(20.0f);
            postFx.UpdateFocalLength(150.0f);
        } else {
            development.SetActive(false);
            accordion = null;
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

            postFx.UpdateAperature(20.0f);
            postFx.UpdateFocalLength(50.0f);
        }

        debugView.gameObject.SetActive(false);
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
        if (trackedImage.trackingState != TrackingState.None)
        {
            trackedImage.transform.localScale = new Vector3(trackedImage.size.x * 0.1f, 0.01f, trackedImage.size.y * 0.1f);

            debugView.UpdateTrackingInformation(trackedImage, arCamera);

            if (showReferenceImage) {
                ShowReferenceImage(trackedImage);
            } else {
                HideReferenceImage(trackedImage);
            }
        } else {
            HideReferenceImage(trackedImage);
        }
    }

    private void UpdateTrackingInformation(ARTrackedImage trackedImage)
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
    }

    private void ShowReferenceImage(ARTrackedImage trackedImage)
    {
        var planeParentGo = trackedImage.transform.Find("PlaneParent").gameObject;
        planeGo = planeParentGo.transform.GetChild(0).gameObject;
        planeGo.SetActive(true);

        var material = planeGo.GetComponentInChildren<MeshRenderer>().material;
        material.mainTexture = trackedImage.referenceImage.texture;
    }

    private void HideReferenceImage(ARTrackedImage trackedImage)
    {
        planeGo.SetActive(false);
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
                    debugView.UpdateStep(step);
                }   
            }

            toggleButton.SetActive(step > 0);
            showReferenceImage = step == 0;

            if (planeGo) {
                planeGo.SetActive(showReferenceImage);
            }
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
