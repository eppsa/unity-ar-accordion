using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class DebugView : MonoBehaviour
{
    [SerializeField] private Text stepText;
    [SerializeField] private Text trackingInformation;

    [SerializeField] Slider focusDistanceSlider;
    [SerializeField] Text focusDistanceValue;
    [SerializeField] Slider apertureSlider;
    [SerializeField] Text apertureValue;
    [SerializeField] Slider focalLengthSlider;
    [SerializeField] Text focalLengthValue;
        
    void Awake() {
        PostFX.OnUpdate += UpdateDepthOfField;
    }

    public void UpdateStep(int step)
    {
        stepText.text = $"Step: {step}";
    }

    public void UpdateTrackingInformation(ARTrackedImage trackedImage, Camera arCamera)
    {
        trackingInformation.text = string.Format("{0}\ntrackingState: {1}\nGUID: {2}\nReference size: {3} cm\nDetected size: {4} cm\nCamera Position: {5}\nTracked Image position: {6}",
            trackedImage.referenceImage.name,
            trackedImage.trackingState,
            trackedImage.referenceImage.guid,
            trackedImage.referenceImage.size * 100f,
            trackedImage.size * 100f, // 0.249 * 100
            arCamera.transform.position,
            trackedImage.transform.position);
    }

    public void UpdateDepthOfField(float focusDistance, float aperture, float focalLength) {
        focusDistanceValue.text = string.Format("Focus distance {0}", focusDistance);

        apertureSlider.value = aperture;
        apertureValue.text = string.Format("Aperture {0}", aperture);

        focalLengthSlider.value = focalLength;
        focalLengthValue.text = string.Format("Focal Length {0}", focalLength);
    }
}
