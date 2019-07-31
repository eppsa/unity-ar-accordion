using UnityEngine;

public class SetTargetFramerate : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Sets the application's target frame rate.")]
    int m_TargetFrameRate = 60;

    public int targetFrameRate
    {
        get { return m_TargetFrameRate; }
        set
        {
            m_TargetFrameRate = value;
            SetFrameRate();
        }
    }

    void SetFrameRate()
    {
        Application.targetFrameRate = targetFrameRate;
    }

    void Start()
    {
        SetFrameRate();
    }
}
