using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


public class PostFX : MonoBehaviour
{
    [SerializeField] PostProcessProfile fxProfile;

    private DepthOfField dof;

    public delegate void UpdateAction(float focusDistance, float aperture, float focalLength);
    public static event UpdateAction OnUpdate;

    void Awake()
    {
        fxProfile.TryGetSettings(out dof);
    }

    void OnStart() {
        OnChange();
    }

    public void UpdateFocusDistance(float distance) {
        dof.focusDistance.value = distance;
        OnChange();
    }

    public void UpdateAperture(float aperture) {
        dof.aperture.value = aperture;
        OnChange();
    }

    public void UpdateFocalLength(float focalLength) {
        dof.focalLength.value = focalLength;
        OnChange();
    }

    private void OnChange() {
        if (OnUpdate != null) {
            OnUpdate(dof.focusDistance.value, dof.aperture.value, dof.focalLength.value);
        }
    }
}
