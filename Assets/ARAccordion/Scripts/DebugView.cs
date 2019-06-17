using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class DebugView : MonoBehaviour
{
    [SerializeField] private Text stepText;
    [SerializeField] private Text trackingInformation;

    public void Refresh(int step)
    {
        stepText.text = $"Step: {step}";
    }

    public void UpdateTrackingInformation(ARTrackedImage trackedImage, Camera arCamera)
    {
        trackingInformation.text = string.Format(
            "{0}\ntrackingState: {1}\nGUID: {2}\nReference size: {3} cm\nDetected size: {4} cm\nCamera Position: {5}\nTracked Image position: {6}",
            trackedImage.referenceImage.name,
            trackedImage.trackingState,
            trackedImage.referenceImage.guid,
            trackedImage.referenceImage.size * 100f,
            trackedImage.size * 100f, // 0.249 * 100
            arCamera.transform.position,
            trackedImage.transform.position);
    }
}
