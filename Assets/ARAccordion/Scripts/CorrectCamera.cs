using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CorrectCamera : MonoBehaviour
{
    private Camera _fxCamera;
    private Camera _sourceCamera;

    private void Awake()
    {
        _sourceCamera = transform.parent.GetComponent<Camera>();
        _fxCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        _fxCamera.projectionMatrix = _sourceCamera.projectionMatrix;
    }

    void OnPreCull()
    {
        _fxCamera.projectionMatrix = _sourceCamera.projectionMatrix;
    }
}
