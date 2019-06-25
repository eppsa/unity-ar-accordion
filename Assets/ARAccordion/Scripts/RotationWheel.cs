using UnityEngine;
using UnityEngine.EventSystems;

public class RotationWheel : MonoBehaviour, IDragHandler
{
    [SerializeField] private GameObject wheelContainer;
    [SerializeField] private RectTransform wheelRectTransform;

    private float minY;
    private float maxY;

    void Start() {
        minY = wheelContainer.transform.position.y;
        maxY = wheelContainer.GetComponent<RectTransform>().rect.height * transform.localScale.y;
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        var newY = Mathf.Max(Mathf.Min(wheelContainer.transform.position.y + eventData.delta.y, maxY), minY);

        wheelContainer.transform.position = new Vector3(
            wheelContainer.transform.position.x, 
            newY, 
            wheelContainer.transform.position.z
        );
    }
}
