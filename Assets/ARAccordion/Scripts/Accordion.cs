using System.Collections;
using UnityEngine;
using Model;
using UnityEngine.Events;

public class Accordion : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject original;
    [SerializeField] private GameObject sections;
    [SerializeField] private GameObject painter;

    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float exponent = 1;

    [SerializeField] private Material defaultSpriteMaterial;
    [SerializeField] private Material dofSpriteMaterial;

    [SerializeField] private UnityEvent onMovementFinish;

    private GameObject[] layers;

    private int sectionsCount;

    private InfoFactory infoFactory;

    private float distanceFactor = 0.5f;

    private bool towardsCamera = true;

    public float step = 0f;
    private int currentLayerIndex = 0;

    private GameObject currentLayerAnchor = null;
    private Transform currentInfoPointAnchors = null;

    private bool savedOrigins = false;

    private Vector3 activeTilePosition;
    private float distanceOfActiveTile;

    private Content content;
    private int startOffset = 0;

    public bool isMoving;
    private bool infoPointsEnabled;
    private Layer currentLayerData;

    public float Exponent { get => exponent; set => exponent = value; }
    public float DistanceFactor { get => distanceFactor; set => distanceFactor = value; }

    public GameObject ActiveImageAnchor { get => currentLayerAnchor; }
    public bool InfoPoinsEnabled { get => infoPointsEnabled; set => infoPointsEnabled = value; }

    void OnEnable()
    {
        this.sectionsCount = sections.transform.childCount;

        this.layers = GameObject.FindGameObjectsWithTag("Layer");

        infoFactory = GetComponent<InfoFactory>();
    }

    void Start()
    {
        background.SetActive(false);

        this.distanceOfActiveTile = GetDistance(this.sectionsCount, 0);
    }

    public void Reset()
    {
        StopAllCoroutines();
        this.step = 0;
    }

    internal void SetStartOffset(int offset)
    {
        this.startOffset = offset;
    }

    public void OnUpdateStep(float step)
    {
        UpdateStep(step);
    }

    private void UpdateStep(float step)
    {
        this.step = step - startOffset;
        Debug.Log(this.step);

        this.currentLayerIndex = Mathf.CeilToInt(this.step) + startOffset;
        Debug.Log("Current layer: " + this.currentLayerIndex);

        this.currentLayerData = this.content.accordion.layers[this.currentLayerIndex];
        Debug.Log("Id: " + this.currentLayerData.id);
        Debug.Log("Type: " + this.currentLayerData.type);

        this.currentLayerAnchor = this.layers[currentLayerIndex].transform.Find("ImageAnchor").gameObject;

        UpdateInfoPoints();
        HighlightImageSections();
    }

    void LateUpdate()
    {
        if (this.step > 0) {
            original.SetActive(false);
            background.SetActive(true);
            sections.SetActive(true);
        } else if (this.step < 0) {
            original.SetActive(true);
            background.SetActive(false);
            sections.SetActive(false);
        } else if (this.step == 0) {
            original.SetActive(true);
            background.SetActive(false);
            sections.SetActive(false);

            ResetToOriginPositions();
        }

        UpdateImageSectionPositions();
        UpdatePainterPosition();

        FocusActiveLayer();
    }

    private void FocusActiveLayer()
    {
        if (this.currentLayerAnchor && this.step != 0 && this.step % 1 == 0) {
            float focusDistance = Vector3.Distance(Camera.main.transform.position, currentLayerAnchor.transform.position);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(focusDistance);
        }
    }

    private void ResetToOriginPositions()
    {
        for (int i = 0; i < this.sectionsCount; i++) {
            GameObject imageAnchor = this.sections.transform.GetChild(i).Find("ImageAnchor").gameObject;

            imageAnchor.transform.localRotation = Quaternion.Euler(0, 0, 0);
            imageAnchor.transform.position = Vector3.zero;
        }
    }

    private void UpdateImageSectionPositions()
    {
        for (int i = 0; i < this.sectionsCount; i++) {
            GameObject imageAnchor = this.sections.transform.GetChild(i).Find("ImageAnchor").gameObject;

            float distance = GetDistance(step, i);

            if (towardsCamera) {
                moveTowardsCamera(imageAnchor, distance);
            } else {
                moveFromOrigin(imageAnchor, distance);
            }

            UpdateMaterial(imageAnchor, i);
        }
    }

    private void UpdatePainterPosition()
    {
        float distance = Mathf.Pow(step + this.sectionsCount + 1, exponent) / Mathf.Pow(this.sectionsCount, exponent);

        if (towardsCamera) {
            moveTowardsCamera(painter, distance);
        } else {
            moveFromOrigin(painter, distance);
        }
    }

    private float GetDistance(float step, int index)
    {
        return Mathf.Pow(step + (this.sectionsCount - 1 - index), exponent) / Mathf.Pow(this.sectionsCount, exponent);
    }

    private void moveFromOrigin(GameObject go, float stepDistance)
    {
        Vector3 origin = go.transform.parent.transform.position;

        float distanceToCamera = Mathf.Abs(Vector3.Distance(Camera.main.transform.position, origin));

        Vector3 newLocalPosition = new Vector3(0, 0, -1) * stepDistance * distanceToCamera * distanceFactor;

        go.transform.localPosition = newLocalPosition;
    }

    private void moveTowardsCamera(GameObject go, float stepDistance)
    {
        Vector3 originPosition = go.transform.parent.transform.position;

        Vector3 distanceVector = Camera.main.transform.position - originPosition;

        Vector3 newPosition = originPosition + distanceVector * distanceFactor * stepDistance;

        go.transform.position = newPosition;

        Vector3 newDirection;
        if (Vector3.Distance(newPosition, originPosition) > 0.2f) {
            newDirection = Vector3.RotateTowards(go.transform.forward, Camera.main.transform.forward, speed * 0.002f * Time.deltaTime, 0.0f);
        } else {
            newDirection = Vector3.RotateTowards(go.transform.forward, go.transform.parent.transform.forward, speed * 0.02f * Time.deltaTime, 0.0f);
        }
        go.transform.rotation = Quaternion.LookRotation(newDirection, new Vector3(0, Camera.main.transform.up.y, 0));
    }

    private void UpdateInfoPoints()
    {
        if (step % 1 == 0) {
            if (infoPointsEnabled) {
                ShowInfoPoints();
            }
        } else {
            HideInfoPoints();
        }
    }

    private void UpdateMaterial(GameObject imageAnchor, int index)
    {
        Layer layerData = this.content.accordion.layers[index + startOffset + 1];

        if (layerData.type == "behind") {
            if (this.currentLayerIndex == index + startOffset + 1) {
                if (this.step >= 9) {
                    imageAnchor.GetComponentInChildren<Renderer>().material = dofSpriteMaterial;
                    Material material = imageAnchor.GetComponentInChildren<Renderer>().material;
                    Color color = material.GetColor("_Color");

                    material.SetColor("_Color", new Color(color.r, color.g, color.b, 1));
                } else if (this.step >= 8) {
                    imageAnchor.GetComponentInChildren<Renderer>().material = defaultSpriteMaterial;
                    Material material = imageAnchor.GetComponentInChildren<Renderer>().material;
                    Color color = material.GetColor("_Color");

                    material.SetColor("_Color", new Color(color.r, color.g, color.b, step % 1));
                }
            } else {
                imageAnchor.GetComponentInChildren<Renderer>().material = defaultSpriteMaterial;
                Material material = imageAnchor.GetComponentInChildren<Renderer>().material;
                Color color = material.GetColor("_Color");

                material.SetColor("_Color", new Color(color.r, color.g, color.b, 0));
            }
        }
    }

    private void ShowInfoPoints()
    {
        Transform infoPointAnchors = currentLayerAnchor.transform.Find("Anchors");

        if (infoPointAnchors) {
            this.currentInfoPointAnchors = infoPointAnchors;
            infoFactory.CreateInfoPoints(content.accordion.layers[this.currentLayerIndex].infos, this.currentInfoPointAnchors);
        }
    }

    private void HideInfoPoints()
    {
        if (this.currentInfoPointAnchors) {
            infoFactory.ClearInfoPoints(this.currentInfoPointAnchors);
        }
    }

    private void HighlightImageSections()
    {
        for (int i = 0; i < this.sectionsCount; i++) {
            GameObject imageAnchor = this.sections.transform.GetChild(i).Find("ImageAnchor").gameObject;

            float distanceOfTile = GetDistance(this.currentLayerIndex, i);

            if (distanceOfTile > this.distanceOfActiveTile) {
                imageAnchor.GetComponentInChildren<Renderer>().material = defaultSpriteMaterial;

                Material material = imageAnchor.GetComponentInChildren<Renderer>().material;
                Color color = material.GetColor("_Color");
                material.SetColor("_Color", new Color(color.r, color.g, color.b, 1 - (step % 1)));
            } else {
                imageAnchor.GetComponentInChildren<Renderer>().material = dofSpriteMaterial;

                Material material = imageAnchor.GetComponentInChildren<Renderer>().material;
                Color color = material.GetColor("_Color");
                material.SetColor("_Color", new Color(color.r, color.g, color.b, 1));
            }
        }
    }

    internal void SetMoveTowardsCamera(bool towardsCamera)
    {
        this.towardsCamera = towardsCamera;
    }

    internal void SetContent(Content content)
    {
        this.content = content;
    }

    public void MoveTo(float to, float duration)
    {
        StartCoroutine(DoMoveTo(to, duration));
    }

    public IEnumerator DoMoveTo(float to, float duration)
    {
        isMoving = true;

        float from = step;

        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true) {
            currentDuration = Time.time - startTime;

            if (duration > 0) {
                progress = currentDuration / duration;
            } else {
                progress = 1.1f;
            }

            if (progress <= 1.0f) {
                UpdateStep(Mathf.Lerp(from, to, progress));
                yield return new WaitForEndOfFrame();
            } else {
                if (to > 0) {
                    UpdateStep(to);
                }
                isMoving = false;
                onMovementFinish.Invoke();
                yield break;
            }
        }
    }
}
