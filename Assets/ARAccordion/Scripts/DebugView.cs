using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugView : MonoBehaviour
{
    [SerializeField] private Text stepText;

    public void Refresh(int step)
    {
        stepText.text = $"Step: {step}";
    }
}
