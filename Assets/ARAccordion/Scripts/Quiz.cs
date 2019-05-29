using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class Quiz : MonoBehaviour
{
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    ARRaycastManager raycastManager;

    void Update()
    {
        if (Input.touchCount == 0)
            return;

        if (raycastManager == null) {
            return;
        }

        var touch = Input.GetTouch(0);

        // var touch = Input.mousePosition;
        // if (touch == null) {
        //     return;
        // }

        // Debug.Log(touch);

        if (raycastManager.Raycast(touch.position, s_Hits, TrackableType.All))
        {
            var hitPose = s_Hits[0].pose;
            transform.position = hitPose.position;
        }
    }

    internal void SetRaycastManager(ARRaycastManager raycastManager)
    {
        this.raycastManager = raycastManager;
    }
}
