using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour
{
    public void UpdateValue(float sliderValue)
    {
        GetComponent<Text>().text = sliderValue.ToString();
    }
}
