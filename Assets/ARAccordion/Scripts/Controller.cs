using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.XR.ARFoundation;
using Newtonsoft.Json;
using Model;
using System.IO;
using System;
using System.Collections;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SpatialTracking;

public class Controller : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private GameObject axes;
    [SerializeField] private StartScreen startScreen;
    [SerializeField] private Camera arCamera;
    [SerializeField] private DebugView debugView;
    [SerializeField] private ToggleButton toggleButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private Camera fxCamera;
    [SerializeField] private Accordion accordion;
    [SerializeField] private RotationWheel rotationWheel;
    [SerializeField] private ARSession arSession;
    [SerializeField] private Quiz quiz;
    [SerializeField] private GameObject screenUI;

    [SerializeField] private float smoothTime;

    private ARTrackedImage trackedImage;

    private int maxDistance;

    private Content content;

    private bool quizActive;
    private bool showReferenceImage = true;
    private bool dofEnabled = true;

    private Vector3 velocity = Vector3.zero;

    private int startLayer = 1;

    private AudioSource clickSound;

    void OnEnable()
    {
        trackedImageManager.enabled = true;

        arCamera.GetComponent<UnityEngine.XR.ARFoundation.ARCameraManager>().focusMode = CameraFocusMode.Fixed;

        maxDistance = GameObject.FindGameObjectsWithTag("Layer").Length;

        rotationWheel.Init(maxDistance);
        rotationWheel.SetStart(startLayer);

        ReadJson();

        accordion.SetContent(this.content);
        accordion.SetStart(startLayer);
        quiz.SetContent(this.content.accordion);

        PostFX postFx = fxCamera.GetComponent<PostFX>();
        if (Application.isEditor) {
            accordion.gameObject.SetActive(true);

            postFx.UpdateAperture(4f);
            postFx.UpdateFocalLength(80.0f);
        } else {
            accordion.gameObject.SetActive(false);

            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

            postFx.UpdateAperture(1.0f); // Original Image 
            postFx.UpdateFocalLength(42.0f); // Original Image  

            // postFx.UpdateAperture(32.0f); // Desktop Image 
            // postFx.UpdateFocalLength(140.0f); // Desktop Image
        }

        toggleButton.gameObject.SetActive(false);
        backButton.SetActive(false);
        rotationWheel.gameObject.SetActive(false);

        debugView.gameObject.SetActive(false);
        debugView.UpdateSmoothTime(smoothTime);
        debugView.UpdateAxes(axes.activeInHierarchy);
        debugView.UpdateXRUpdateType((int)arCamera.GetComponent<TrackedPoseDriver>().updateType);
        debugView.UpdateAccordionExponent(accordion.Exponent);
        debugView.UpdateDOF(enabled);

        fxCamera.GetComponent<PostProcessLayer>().enabled = false;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added) {
            AddTrackedImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated) {
            UpdateTrackedImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed) {
            accordion.gameObject.SetActive(false);
        }
    }

    private void AddTrackedImage(ARTrackedImage trackedImage)
    {
        this.trackedImage = trackedImage;

        accordion.gameObject.SetActive(true);

        axes.transform.position = trackedImage.transform.position;
        axes.transform.rotation = trackedImage.transform.rotation;
        axes.transform.localScale = new Vector3(this.trackedImage.size.y * 0.5f, this.trackedImage.size.y * 0.5f, this.trackedImage.size.y * 0.5f);

        accordion.transform.position = trackedImage.transform.position;
        accordion.transform.rotation = trackedImage.transform.rotation;
        accordion.transform.localScale = new Vector3(this.trackedImage.size.y, 1f, this.trackedImage.size.y); //2.739377
    }

    void OnDisable()
    {
        StopTracking();
    }

    void StopTracking()
    {
        if (trackedImageManager) {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
            trackedImageManager.enabled = false;
        }
    }

    private void UpdateTrackedImage(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState != TrackingState.None) {
            this.trackedImage = trackedImage;

            debugView.UpdateTrackingInformation(trackedImage, arCamera, accordion);
        } else {
            accordion.gameObject.SetActive(false);
        }
    }

    void ReadJson()
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "content.json");
        string jsonString = File.ReadAllText(jsonPath);
        content = JsonConvert.DeserializeObject<Content>(jsonString);
    }

    void Update()
    {
        if (this.trackedImage != null) {
            accordion.transform.position = Vector3.SmoothDamp(accordion.transform.position, this.trackedImage.transform.position, ref velocity, smoothTime);
            accordion.transform.rotation = Quaternion.RotateTowards(accordion.transform.rotation, this.trackedImage.transform.rotation, smoothTime);

            axes.transform.position = Vector3.SmoothDamp(axes.transform.position, this.trackedImage.transform.position, ref velocity, smoothTime);
            axes.transform.rotation = Quaternion.RotateTowards(axes.transform.rotation, this.trackedImage.transform.rotation, smoothTime);
        }
    }

    public void OnStart()
    {
        rotationWheel.gameObject.SetActive(true);
        toggleButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);
    }

    public void OnActivateTowardsCamera(bool active)
    {
        if (accordion) {
            accordion.SetMoveTowardsCamera(active);
        }
    }

    public void OnToggleQuiz()
    {
        if (!accordion.isMoving) {
            quizActive = !quizActive;

            rotationWheel.Toggle(!quizActive);
            toggleButton.Toggle(quizActive);
            accordion.EnableInfoTags(!quizActive);

            if (quizActive) {
                accordion.DistanceFactor = 0.3f;
                quiz.transform.SetParent(accordion.transform);
            } else {
                accordion.DistanceFactor = 0.5f;
                StartCoroutine(ResetAccordion(false));
            }

            quiz.gameObject.SetActive(quizActive);
        }
    }

    public void OnEnableDofDebugging(bool enable)
    {
        arCamera.GetComponent<PostProcessDebug>().enabled = enable;
        if (accordion != null) {
            accordion.transform.Find("Plane").gameObject.SetActive(enable);
        }
    }

    public void OnSmoothTimeChange(float smoothTime)
    {
        this.smoothTime = smoothTime;
        debugView.UpdateSmoothTime(smoothTime);
    }

    public void OnShowAxis(bool enable)
    {
        axes.SetActive(enable);
    }

    public void OnUpdateTypeChange(Int32 updateType)
    {
        arCamera.GetComponent<TrackedPoseDriver>().updateType = (TrackedPoseDriver.UpdateType)updateType;
    }

    public void OnUpdateRotationWheel(float value)
    {
        float layerIndex = value - startLayer;

        accordion.OnUpdateStep(layerIndex);
        debugView.UpdateStep(layerIndex);

        if (dofEnabled) {
            fxCamera.GetComponent<PostProcessLayer>().enabled = layerIndex != 0;
        }
    }

    public void OnAccordionExponentChange(float exponent)
    {
        accordion.Exponent = exponent;
        debugView.UpdateAccordionExponent(exponent);
    }

    public void OnEnableDof(bool enable)
    {
        dofEnabled = enable;
        fxCamera.GetComponent<PostProcessLayer>().enabled = enable;
    }

    public void OnBackButton()
    {
        if (quizActive) {
            OnToggleQuiz();
        }

        rotationWheel.gameObject.SetActive(false);
        toggleButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        StartCoroutine(ResetAccordion(true));
    }

    IEnumerator ResetAccordion(bool showStartScreen)
    {
        if (accordion.step > 0.1 || accordion.step < 0) {
            screenUI.SetActive(false);
            StartCoroutine(accordion.MoveToLayer(0));
            while (accordion.isMoving) {
                yield return null;
            }

            screenUI.SetActive(true);
        }

        if (showStartScreen) {
            startScreen.gameObject.SetActive(true);
            startScreen.Show(true);
            rotationWheel.Toggle(false);
        }
    }
}
