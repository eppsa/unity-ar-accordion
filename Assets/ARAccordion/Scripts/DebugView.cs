using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class DebugView : MonoBehaviour
{
    [SerializeField] private Text stepText;
    [SerializeField] private Text trackingInformation;

    [SerializeField] Text focusDistanceValue;
    [SerializeField] Slider apertureSlider;
    [SerializeField] Text apertureValue;
    [SerializeField] Slider focalLengthSlider;
    [SerializeField] Text focalLengthValue;
    [SerializeField] Slider smoothTimeSlider;
    [SerializeField] Text smoothTimeValue;
    [SerializeField] Slider exponentSlider;
    [SerializeField] Text exponentValue;
    [SerializeField] Slider rotationWheelSpeedSlider;
    [SerializeField] Text rotationWheelSpeedValue;

    [SerializeField] Toggle axes;
    [SerializeField] Toggle dof;

    [SerializeField] Dropdown updateType;

    [SerializeField] private UnityEvent onRotationWheelSpeedChange;


    void Awake()
    {
        PostFX.OnUpdate += UpdateDepthOfField;
    }

    public void UpdateStep(float step)
    {
        stepText.text = $"Step: {step}";
    }

    public void UpdateTrackingInformation(ARTrackedImage trackedImage, Camera arCamera, Accordion accordion)
    {
        trackingInformation.text = string.Format(
            "{0}\n" +
            "trackingState: {1}\n" +
            "GUID: {2}\n" +
            "Reference size: {3} cm\n" +
            "Detected size: {4} cm\n" +
            "Camera Position: {5}\n" +
            "Tracked Image position: {6}\n" +
            "Tracked Image Local/ Lossy Scale: {7}\n" +
            "Accordion Local/ Lossy Scale: {8}",
            trackedImage.referenceImage.name,
            trackedImage.trackingState,
            trackedImage.referenceImage.guid,
            trackedImage.referenceImage.size * 100f,
            trackedImage.size * 100f, // 0.249 * 100
            arCamera.transform.position,
            trackedImage.transform.position,
            trackedImage.transform.localScale,
            trackedImage.transform.lossyScale,
            accordion.transform.localScale,
            accordion.transform.lossyScale);
    }

    public void UpdateDepthOfField(float focusDistance, float aperture, float focalLength)
    {
        focusDistanceValue.text = string.Format("Focus distance {0}", focusDistance);

        apertureSlider.value = aperture;
        apertureValue.text = string.Format("Aperture {0}", aperture);

        focalLengthSlider.value = focalLength;
        focalLengthValue.text = string.Format("Focal Length {0}", focalLength);
    }

    internal void UpdateAccordionExponent(float exponent)
    {
        exponentSlider.value = exponent;
        exponentValue.text = string.Format("Accordion Exponent {0}", exponent);
    }

    internal void UpdateRotationWheelSpeed(float speed)
    {
        rotationWheelSpeedSlider.value = speed;
        rotationWheelSpeedValue.text = string.Format("Rotation Wheel Speed {0}", speed);
    }

    internal void UpdateDOF(bool enabled)
    {
        dof.isOn = enabled;
    }

    internal void UpdateAxes(bool enabled)
    {
        axes.isOn = enabled;
    }

    public void UpdateSmoothTime(float smoothTime)
    {
        smoothTimeSlider.value = smoothTime;
        smoothTimeValue.text = string.Format("Smooth time {0}", smoothTime);
    }
}
