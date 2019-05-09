using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CheckDeviceSupport : MonoBehaviour {
    [SerializeField] ARSession m_Session;

    IEnumerator Start() {
        if ((ARSession.state == ARSessionState.None) ||
            (ARSession.state == ARSessionState.CheckingAvailability))
        {
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            // Start some fallback experience for unsupported devices
            Debug.Log("Device is unsupported.");
        }
        else
        {
            // Start the AR session
            m_Session.enabled = true;
        }
    }
}