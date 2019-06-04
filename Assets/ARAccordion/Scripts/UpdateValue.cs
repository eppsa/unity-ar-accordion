using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateValue : MonoBehaviour
{
    public void OnValueChanged(float value) {
        GetComponent<Text>().text = $"{value}";
    }
}
