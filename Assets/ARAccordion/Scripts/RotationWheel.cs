using UnityEngine;
using UnityEngine.EventSystems;

public class RotationWheel : MonoBehaviour, IDragHandler, IDropHandler
{
    [SerializeField] private GameObject wheelContainer;
    [SerializeField] private GameObject wheelElementPrefab;
    [SerializeField] private Controller controller;

    private float minY;
    private float maxY;

    private int maxSteps = 3;
    private Vector3 nextStepLocalPosition;
    private float wheelElementHeight;
    private bool dragging = false;

    private float value;

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        dragging = true;

        float newLocalY = Mathf.Max(Mathf.Min(wheelContainer.transform.localPosition.y + eventData.delta.y, maxY), 0);

        wheelContainer.transform.localPosition = new Vector3(
            wheelContainer.transform.localPosition.x,
            newLocalY,
            wheelContainer.transform.localPosition.z
        );
    }

    void IDropHandler.OnDrop(PointerEventData eventData)
    {
        float newLocalY = Mathf.Max(Mathf.Min(wheelContainer.transform.localPosition.y + eventData.delta.y, maxY), 0);

        int step = Mathf.RoundToInt(newLocalY / wheelElementHeight);

        nextStepLocalPosition = new Vector3(
            wheelContainer.transform.localPosition.x,
            step * wheelElementHeight,
            wheelContainer.transform.localPosition.z
        );

        dragging = false;
    }

    void Start()
    {
        nextStepLocalPosition = wheelContainer.transform.localPosition;
        wheelElementHeight = wheelElementPrefab.GetComponent<RectTransform>().sizeDelta.y;
        Debug.Log(wheelElementHeight);
    }

    void Update()
    {
        if (!dragging) {
            wheelContainer.transform.localPosition = Vector3.MoveTowards(wheelContainer.transform.localPosition, nextStepLocalPosition, Time.deltaTime * 200f);
        }

        float newValue = wheelContainer.transform.localPosition.y / wheelElementHeight;

        if (newValue != this.value) {
            controller.OnUpdateRotationWheel(newValue);
            this.value = newValue;
        }
    }

    internal void Init(int maxSteps)
    {
        this.maxSteps = maxSteps;

        for (int i = 0; i <= maxSteps; i++) {
            GameObject wheelElement = Instantiate(wheelElementPrefab);
            wheelElement.transform.SetParent(wheelContainer.transform, false);

            wheelElement.name = "WheelElement" + i;
        }

        maxY = wheelElementPrefab.GetComponent<RectTransform>().sizeDelta.y * maxSteps;
    }
}
