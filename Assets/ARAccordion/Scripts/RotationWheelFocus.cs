using UnityEngine;
using UnityEngine.Events;

public class RotationWheelFocus : MonoBehaviour
{
    [SerializeField] UnityEvent rotationWheelSnapEvent;

    public void OnTriggerEnter2D()
    {
        rotationWheelSnapEvent.Invoke();
    }
}
