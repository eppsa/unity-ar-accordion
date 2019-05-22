using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSettings : MonoBehaviour
{
    public void updateCanvasPosition(Vector3 newPosition) {
        Debug.Log("update layer");
        this.gameObject.transform.position = newPosition;
    }
}
