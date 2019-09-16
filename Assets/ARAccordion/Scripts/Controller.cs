using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.XR.ARFoundation;
using Newtonsoft.Json;
using Model;
using System.IO;
using System;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SpatialTracking;

public enum State
{
    START,
    ACCORDION,
    QUIZ
}

public class Controller : MonoBehaviour
{
    private const int RESET_TIMEOUT = 60;

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

    private Content content;

    private bool quizActive;
    private bool showReferenceImage = true;
    private bool dofEnabled = true;

    private Vector3 velocity = Vector3.zero;

    private int startLayer = 1;

    private AudioSource clickSound;

    private State state;

    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;

    bool resetTimerStarted;
    float timerStartTime;

    void OnEnable()
    {
        Init();

        this.state = State.START;
        UpdateState();
    }

    void Init()
    {
        arCamera.GetComponent<UnityEngine.XR.ARFoundation.ARCameraManager>().focusMode = CameraFocusMode.Fixed;

        ReadJson();

        rotationWheel.Init(content.accordion.layers, startLayer);

        accordion.SetContent(this.content);
        accordion.SetStartOffset(startLayer);
        accordion.SetStep(startLayer);

        quiz.SetContent(this.content.accordion);

        PostFX postFx = fxCamera.GetComponent<PostFX>();
        if (Application.isEditor) {
            postFx.UpdateAperture(4f);
            postFx.UpdateFocalLength(80.0f);
        } else {
            postFx.UpdateAperture(1.0f);
            postFx.UpdateFocalLength(42.0f);

            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }

        debugView.gameObject.SetActive(false);
        debugView.UpdateSmoothTime(smoothTime);
        debugView.UpdateAxes(axes.activeInHierarchy);
        debugView.UpdateAccordionExponent(accordion.Exponent);
        debugView.UpdateDOF(enabled);
        debugView.UpdateRotationWheelSpeed(0.5f);

        fxCamera.GetComponent<PostProcessLayer>().enabled = true;
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
        UpdateResetTimer();

        if (this.trackedImage != null) {
            accordion.transform.position = Vector3.SmoothDamp(accordion.transform.position, this.trackedImage.transform.position, ref velocity, smoothTime);
            accordion.transform.rotation = Quaternion.RotateTowards(accordion.transform.rotation, this.trackedImage.transform.rotation, smoothTime);

            axes.transform.position = Vector3.SmoothDamp(axes.transform.position, this.trackedImage.transform.position, ref velocity, smoothTime);
            axes.transform.rotation = Quaternion.RotateTowards(axes.transform.rotation, this.trackedImage.transform.rotation, smoothTime);
        }
    }

    private void UpdateResetTimer()
    {
        if (Application.isEditor) {
            return;
        }

        if (this.state == State.START) {
            return;
        }

        if (!resetTimerStarted) {
            savedCameraPosition = arCamera.transform.position;
            savedCameraRotation = arCamera.transform.rotation;
            timerStartTime = Time.time;
            resetTimerStarted = true;
        }

        if (Input.touchCount > 0 || Input.GetMouseButton(0)) {
            Debug.Log("Touched. Reset timer");
            resetTimerStarted = false;
        }

        if (Time.time > timerStartTime + RESET_TIMEOUT) {
            float distance = Vector3.Distance(arCamera.transform.position, savedCameraPosition);
            float angle = Quaternion.Angle(arCamera.transform.rotation, savedCameraRotation);

            if (distance < 0.1f && angle < 1.0f) {
                this.state = State.START;
                UpdateState();

                Debug.Log("Reset.");
            }

            resetTimerStarted = false;
        };
    }

    public void OnActivateTowardsCamera(bool active)
    {
        if (accordion) {
            accordion.SetMoveTowardsCamera(active);
        }
    }

    public void OnToggleMode()
    {
        if (this.state == State.ACCORDION) {
            this.state = State.QUIZ;
        } else if (this.state == State.QUIZ) {
            this.state = State.ACCORDION;
        }

        Debug.Log("OnToggleMode()");

        UpdateState();
    }

    public void OnStartQuiz()
    {
        this.state = State.QUIZ;
        UpdateState();
    }

    private void UpdateState()
    {
        switch (this.state) {
            case State.START:
                DeactivateTracking();
                ShowStart();
                break;
            case State.ACCORDION:
                ActivateTracking();
                ShowAccordion();
                break;
            case State.QUIZ:
                ActivateTracking();
                ShowQuiz();
                break;
        }
    }

    private void ShowStart()
    {
        screenUI.SetActive(false);
        startScreen.gameObject.SetActive(true);
        startScreen.Show(true);

        accordion.gameObject.SetActive(false);
        quiz.gameObject.SetActive(false);

        accordion.Reset();
    }

    private void ShowAccordion()
    {
        accordion.StopAllCoroutines();

        quiz.gameObject.SetActive(false);
        screenUI.SetActive(true);

        rotationWheel.gameObject.SetActive(true);
        accordion.InfoPoinsEnabled = true;

        if (startScreen.isActiveAndEnabled) {
            startScreen.Show(false);
        }

        accordion.DistanceFactor = 0.5f;

        rotationWheel.Reset();

        toggleButton.Toggle(state);

        accordion.MoveTo(startLayer, accordion.Step != startLayer ? 1.5f : 0);
    }

    private void ShowQuiz()
    {
        quiz.Reset();
        accordion.StopAllCoroutines();

        quiz.transform.SetParent(accordion.gameObject.transform);
        quiz.gameObject.SetActive(true);
        screenUI.SetActive(true);
        rotationWheel.gameObject.SetActive(false);
        accordion.InfoPoinsEnabled = false;

        accordion.DistanceFactor = 0.3f;

        toggleButton.Toggle(state);

        accordion.MoveTo(startLayer, accordion.Step != startLayer ? 1.5f : 0);
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
        accordion.OnUpdateStep(value);
        debugView.UpdateStep(value);
    }

    public void OnAccordionExponentChange(float exponent)
    {
        accordion.Exponent = exponent;
        debugView.UpdateAccordionExponent(exponent);
    }

    public void OnRotationWheelSpeedChange(float speed)
    {
        rotationWheel.Speed = speed;
        debugView.UpdateRotationWheelSpeed(speed);
    }

    public void OnEnableDof(bool enable)
    {
        dofEnabled = enable;
        fxCamera.GetComponent<PostProcessLayer>().enabled = enable;
    }

    public void OnBackButton()
    {
        this.state = State.START;
        UpdateState();
    }

    public void OnStart()
    {
        Debug.Log("OnStart");
        this.state = State.ACCORDION;
        UpdateState();
    }

    private void DeactivateTracking()
    {
        trackedImageManager.enabled = false;
    }

    private void ActivateTracking()
    {
        if (!trackedImageManager.enabled) {
            if (Application.isEditor) {
                accordion.gameObject.SetActive(true);
            } else {
                accordion.gameObject.SetActive(false);
            }

            trackedImageManager.enabled = true;
        }
    }

    public void OnAccordionMovementFinish()
    {
        switch (state) {
            case State.QUIZ:
                quiz.OnAccordionMovementFinsh();
                break;
        }
    }
}
