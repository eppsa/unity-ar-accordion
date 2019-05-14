using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.XR.ARFoundation;


public class PostFX : MonoBehaviour
{
    [SerializeField] Camera camera;
    [SerializeField] PostProcessProfile fxProfile;
    [SerializeField] ARSessionOrigin sessionOrigin;

    private DepthOfField dof;

    // Start is called before the first frame update
    void Start()
    {
        fxProfile.TryGetSettings(out dof);
    }

    // Update is called once per frame
    void Update()
    {
        Transform transform = sessionOrigin.trackablesParent.GetChild(0);

        if (transform != null) {
            // Debug.Log("Child count: " + transform.childCount);
            // Debug.Log("Child " + transform.GetChild(0).gameObject);
            // Debug.Log("Juno " + transform.GetChild(0).Find("Juno"));
            Transform womanNaked = transform.GetChild(0).Find("WomanNaked"); 
            // Debug.Log("WomanNaked " + womanNaked.position); // (0.1, -0.1, 0.2)
            // Debug.Log("WomanNaked local " + womanNaked.localPosition); // 0.7, 0.9, -13.8

            // Debug.Log("Camera" + camera.transform.localPosition); 

            float distance = Vector3.Distance(camera.transform.localPosition, womanNaked.position);
            Debug.Log("Distance: " + distance);

            // UpdateFocusDistance(distance);

            // ARTrackedImage trackedImage = transform.gameObject.GetComponent<ARTrackedImage>();
            // Debug.Log(trackedImage.transform.localPosition);
        }

        // GameObject go = GameObject.FindGameObjectWithTag("juno");
        // if (transform == null) {
        //     return;
        // }

        // Debug.Log(go.transform.localPosition);

        // Debug.Log(sessionOrigin.trackablesParent.Find("Juno").transform.position);
        // Debug.Log("Juno " + sessionOrigin.trackablesParent.Find("Juno").transform.localPosition);

        // Debug.Log("Camera" + camera.transform.position);

        // Debug.Log(juno.transform.position);
        
    }

    public void UpdateFocusDistance(float distance) {
        // var targetFocusRange = distance / .4f;
        // var targetFocusDistance = distance - (distance / 4);
        // if( distance < 3 ){
        //     targetFocusRange= distance;
        //     targetFocusDistance= distance - .5f;
        //         if(distance< .7f){
        //             targetFocusRange=.5f;
        //             targetFocusDistance=.4f;
        //         }
        // }
        // dof.focalRange = targetFocusRange;
        // dof.focalDistance = targetFocusDistance;

        // DepthOfField dof;
        // fxProfile.TryGetSettings(out dof);

        dof.focusDistance.value = distance;
        // dof.focalLength.value = targetFocusRange;
    }

    public void UpdateAperature(float aperture) {
        dof.aperture.value = aperture;
    }

    public void UpdateFocalLength(float focalLength) {
        dof.focalLength.value = focalLength;
    }
}
