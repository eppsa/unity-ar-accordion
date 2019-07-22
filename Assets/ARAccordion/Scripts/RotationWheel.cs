using UnityEngine;
using UnityEngine.EventSystems;

public class RotationWheel : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject wheelContainer;
    [SerializeField] private GameObject wheelElementPrefab;
    [SerializeField] private Controller controller;

    private float minY;
    private float maxY;

    private int maxSteps;
    private Vector3 nextStepLocalPosition;
    private float wheelElementHeight;
    private bool dragging = false;

    private float step;
    private float start = 0;

    internal void Init(int maxSteps)
    {
        this.maxSteps = maxSteps;

        for (int i = 0; i <= maxSteps; i++) {
            GameObject wheelElement = Instantiate(wheelElementPrefab);
            wheelElement.transform.SetParent(wheelContainer.transform, false);

            wheelElement.name = "WheelElement" + i;
        }

        wheelElementHeight = wheelElementPrefab.GetComponent<RectTransform>().sizeDelta.y;
        maxY = wheelElementHeight * maxSteps;

        nextStepLocalPosition = wheelContainer.transform.localPosition;
    }

    internal void SetStart(int start)
    {
        this.start = start;
        nextStepLocalPosition = wheelContainer.transform.localPosition + new Vector3(
            wheelContainer.transform.localPosition.x,
            this.start * wheelElementHeight,
            wheelContainer.transform.localPosition.z
        );
        wheelContainer.transform.localPosition = nextStepLocalPosition;
    }

    void Update()
    {
        if (!dragging) {
            wheelContainer.transform.localPosition = Vector3.MoveTowards(wheelContainer.transform.localPosition, nextStepLocalPosition, Time.deltaTime * 200f);
        }

        float step = wheelContainer.transform.localPosition.y / wheelElementHeight;

        if (step != this.step) {
            controller.OnUpdateRotationWheel(step);
            this.step = step;
        }
    }

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

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
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
}
