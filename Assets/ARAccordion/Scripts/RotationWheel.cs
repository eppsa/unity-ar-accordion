using UnityEngine;
using UnityEngine.EventSystems;

public class RotationWheel : MonoBehaviour, IDragHandler
{
    [SerializeField] private GameObject wheelContainer;
    [SerializeField] private RectTransform wheelRectTransform;

    private float minY;
    private float maxY;

    void OnEnable() {

    }

    void Start() {
        minY = wheelContainer.transform.position.y;
        // maxY = wheelContainer.GetComponent<RectTransform>().rect.height * transform.localScale.y;
        maxY = wheelRectTransform.sizeDelta.y;

        // Debug.Log(wheelRectTransform.rect.height);
        Debug.Log(minY);
        Debug.Log(maxY);
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        // var newY = Mathf.Max(Mathf.Min(wheelContainer.transform.position.y + eventData.delta.y, maxY), minY);

        var newLocalY = Mathf.Max(Mathf.Min(wheelContainer.transform.localPosition.y + eventData.delta.y, maxY), 0);
        

        // Debug.Log(newY);
        Debug.Log(newLocalY);


        // wheelContainer.transform.position = new Vector3(
        //     wheelContainer.transform.position.x, 
        //     newY, 
        //     wheelContainer.transform.position.z
        // );

        wheelContainer.transform.localPosition = new Vector3(
            wheelContainer.transform.localPosition.x, 
            newLocalY, 
            wheelContainer.transform.localPosition.z
        );
    }
}
