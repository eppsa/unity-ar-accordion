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
    [SerializeField] private GameObject axis;
    [SerializeField] private Camera arCamera;
    [SerializeField] private DebugView debugView;
    [SerializeField] private GameObject toggleButton;
    [SerializeField] private PostFX postFx;
    [SerializeField] private Accordion accordion;

    private ARTrackedImage arTrackedImage;

    private int maxSteps;
    private int step;

    private Content content;

    private bool quizActive;
    private GameObject planeGo;
    private bool showReferenceImage = true;

    private Vector3 velocity = Vector3.zero;

    private Vector3 newPosition;
    private Quaternion newRotation;

    void OnEnable() {
        maxSteps = accordion.transform.Find("Content").childCount;

        ReadJson();

        accordion.SetContent(this.content);

        if (Application.isEditor) {
            postFx.UpdateAperature(20.0f);
            postFx.UpdateFocalLength(150.0f);
        } else {
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
        axis.transform.position = trackedImage.transform.position;
        axis.transform.rotation = trackedImage.transform.rotation;

        accordion.transform.position = trackedImage.transform.position;
        accordion.transform.rotation = trackedImage.transform.rotation;
    }

    private void UpdateTrackedImage(ARTrackedImage trackedImage)
    {
        debugView.UpdateTrackingInformation(trackedImage, arCamera);

        if (trackedImage.trackingState != TrackingState.None)
        {
            newPosition = trackedImage.transform.position;
            newRotation = trackedImage.transform.rotation;
            accordion.transform.localScale = trackedImage.transform.localScale = new Vector3(trackedImage.size.x * 0.01f, 0.01f, trackedImage.size.y * 0.01f);

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

        if (newPosition != null && newRotation != null) {
            accordion.transform.position = Vector3.SmoothDamp(accordion.transform.position, newPosition, ref velocity, 2f);
            accordion.transform.rotation = Quaternion.Slerp(accordion.transform.rotation, newRotation, 2f);

            axis.transform.position = Vector3.SmoothDamp(axis.transform.position, newPosition, ref velocity, 0.5f);
            axis.transform.rotation = Quaternion.Slerp(axis.transform.rotation, newRotation, 0.5f);
        }
        // accordion.transform.localScale = 
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
