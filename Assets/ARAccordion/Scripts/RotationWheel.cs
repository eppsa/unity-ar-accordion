using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class RotationWheel : MonoBehaviour, IDragHandler
{
    [SerializeField] private GameObject wheelContainer;
    [SerializeField] private GameObject wheelElementPrefab;

    private float minY;
    private float maxY;

    private int maxSteps = 3;


    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        var newLocalY = Mathf.Max(Mathf.Min(wheelContainer.transform.localPosition.y + eventData.delta.y, maxY), 0);

        wheelContainer.transform.localPosition = new Vector3(
            wheelContainer.transform.localPosition.x, 
            newLocalY, 
            wheelContainer.transform.localPosition.z
        );
    }

    internal void Init(int maxSteps)
    {
        this.maxSteps = maxSteps;

        for (int i = 0; i < maxSteps; i++)
        {
            GameObject wheelElement = Instantiate(wheelElementPrefab);   
            wheelElement.transform.SetParent(wheelContainer.transform, false); 

            wheelElement.name = "WheelElement" + i;
        }

        maxY = wheelElementPrefab.GetComponent<RectTransform>().sizeDelta.y * (maxSteps - 1);
    }
}
