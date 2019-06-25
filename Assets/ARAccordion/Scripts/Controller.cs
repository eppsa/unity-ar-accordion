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
using UnityEngine.SpatialTracking;

public class Controller : MonoBehaviour
{
    [SerializeField] ARTrackedImageManager trackedImageManager;
    [SerializeField] private GameObject axes;
    [SerializeField] private Camera arCamera;
    [SerializeField] private DebugView debugView;
    [SerializeField] private GameObject toggleButton;
    [SerializeField] private Camera fxCamera;
    [SerializeField] private Accordion accordion;
    [SerializeField] private RotationWheel rotationWheel;

    [SerializeField] private float smoothTime = 10.0f;

    private ARTrackedImage trackedImage;

    private int maxSteps;
    private int step;

    private Content content;

    private bool quizActive;
    private GameObject referenceImagePlane;
    private bool showReferenceImage = true;

    private Vector3 velocity = Vector3.zero;

    void OnEnable() {
        arCamera.GetComponent<UnityEngine.XR.ARFoundation.ARCameraManager>().focusMode = CameraFocusMode.Fixed;

        maxSteps = accordion.transform.Find("Content").childCount;

        rotationWheel.Init(maxSteps);

        ReadJson();

        accordion.SetContent(this.content);

        PostFX postFx = fxCamera.GetComponent<PostFX>();
        if (Application.isEditor) {
            postFx.UpdateAperture(20.0f);
            postFx.UpdateFocalLength(150.0f);
        } else {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

            postFx.UpdateAperture(20.0f);
            postFx.UpdateFocalLength(50.0f);
        }

        debugView.gameObject.SetActive(false);
        debugView.UpdateSmoothTime(smoothTime);
        debugView.UpdateAxes(axes.activeInHierarchy);
        debugView.UpdateDOF(fxCamera.GetComponent<PostProcessLayer>().enabled);
        debugView.UpdateXRUpdateType((int) arCamera.GetComponent<TrackedPoseDriver>().updateType);
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
        this.trackedImage = trackedImage;

        axes.transform.position = trackedImage.transform.position;
        axes.transform.rotation = trackedImage.transform.rotation;
        axes.transform.localScale = new Vector3(this.trackedImage.size.y * 0.5f, this.trackedImage.size.y * 0.5f, this.trackedImage.size.y * 0.5f);
 
        accordion.transform.position = trackedImage.transform.position;
        accordion.transform.rotation = trackedImage.transform.rotation;
        accordion.transform.localScale = new Vector3(this.trackedImage.size.x * 0.1f, 0.1f, this.trackedImage.size.y * 0.1f);

        referenceImagePlane = trackedImage.transform.Find("ReferenceImagePlane").gameObject;
        referenceImagePlane.transform.localScale = new Vector3(trackedImage.size.x * 0.1f, 0.01f, trackedImage.size.y * 0.1f);
    }

    private void UpdateTrackedImage(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState != TrackingState.None)
        {
            this.trackedImage = trackedImage;

            debugView.UpdateTrackingInformation(trackedImage, arCamera, accordion);

            if (showReferenceImage) {
                ShowReferenceImage(trackedImage);
            } else {
                HideReferenceImage(trackedImage);
            }
        } else {
            HideReferenceImage(trackedImage);
        }
    }

    private void ShowReferenceImage(ARTrackedImage trackedImage)
    {
        referenceImagePlane.SetActive(true);

        var material = referenceImagePlane.GetComponentInChildren<MeshRenderer>().material;
        material.mainTexture = trackedImage.referenceImage.texture;

        Debug.Log(trackedImage.referenceImage.texture);
    }

    private void HideReferenceImage(ARTrackedImage trackedImage)
    {
        referenceImagePlane.SetActive(false);
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

            if (referenceImagePlane) {
                referenceImagePlane.SetActive(showReferenceImage);
            }
        }
    }

    void FixedUpdate()
    {
        if (this.trackedImage != null) {
            accordion.transform.position = Vector3.SmoothDamp(accordion.transform.position, this.trackedImage.transform.position, ref velocity, smoothTime);
            accordion.transform.rotation = Quaternion.RotateTowards(accordion.transform.rotation, this.trackedImage.transform.rotation, smoothTime);

            axes.transform.position = Vector3.SmoothDamp(axes.transform.position, this.trackedImage.transform.position, ref velocity, smoothTime);
            axes.transform.rotation = Quaternion.RotateTowards(axes.transform.rotation, this.trackedImage.transform.rotation, smoothTime);
            axes.transform.localScale = new Vector3(this.trackedImage.size.y * 0.5f, this.trackedImage.size.y * 0.5f, this.trackedImage.size.y * 0.5f);
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

    public void OnSmoothTimeChange(float smoothTime) {
        this.smoothTime = smoothTime;
        debugView.UpdateSmoothTime(smoothTime);
    }

    public void OnShowAxis(bool enable) {
        axes.SetActive(enable);
    }

    public void OnUpdateTypeChange(Int32 updateType) {
        arCamera.GetComponent<TrackedPoseDriver>().updateType = (TrackedPoseDriver.UpdateType) updateType; 
        Debug.Log("OnUpdateTypeChange: " + updateType);
    }
}
