using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.XR.ARFoundation;


public class PostFX : MonoBehaviour
{
    [SerializeField] PostProcessProfile fxProfile;

    private DepthOfField dof;

    void Start()
    {
        fxProfile.TryGetSettings(out dof);
    }

    public void UpdateFocusDistance(float distance) {
        dof.focusDistance.value = distance;
    }

    public void UpdateAperature(float aperture) {
        dof.aperture.value = aperture;
    }

    public void UpdateFocalLength(float focalLength) {
        dof.focalLength.value = focalLength;
    }
}
