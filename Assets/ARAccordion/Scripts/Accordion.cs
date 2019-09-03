using System.Collections;
using UnityEngine;
using Model;
using UnityEngine.Events;
using System;

public class Accordion : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject original;
    [SerializeField] private GameObject layers;

    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float exponent = 1;

    [SerializeField] private Material defaultSpriteMaterial;
    [SerializeField] private Material dofSpriteMaterial;

    [SerializeField] private UnityEvent onMovementFinish;

    private InfoFactory infoFactory;

    private float distanceFactor = 0.5f;

    private bool towardsCamera = true;

    public float step = 0f;
    private int currentLayerIndex = 0;

    private GameObject currentLayerAnchor = null;
    private Transform currentInfoPointAnchors = null;

    private bool savedOrigins = false;

    private Vector3 activeTilePosition;
    private float currentLayerDistance;

    private Content content;
    private int startOffset = 0;

    public bool isMoving;
    private bool infoPointsEnabled;
    private Layer layerData;

    public float Exponent { get => exponent; set => exponent = value; }
    public float DistanceFactor { get => distanceFactor; set => distanceFactor = value; }

    public GameObject ActiveImageAnchor { get => currentLayerAnchor; }
    public bool InfoPoinsEnabled { get => infoPointsEnabled; set => infoPointsEnabled = value; }

    void OnEnable()
    {
        infoFactory = GetComponent<InfoFactory>();
    }

    void Start()
    {
        UpdateStep(0);
        this.currentLayerDistance = GetDistance(1, 0);
    }

    private void UpdateStep(float step)
    {
        this.step = step - startOffset;
        Debug.Log("Step: " + this.step);

        this.currentLayerIndex = Mathf.CeilToInt(this.step) + startOffset;
        Debug.Log("Current layer index: " + this.currentLayerIndex);

        this.layerData = this.content.accordion.layers[this.currentLayerIndex];
        Debug.Log("Id: " + this.layerData.id);

        this.currentLayerAnchor = this.layers.transform.GetChild(currentLayerIndex).Find("ImageAnchor").gameObject;

        // UpdateInfoPoints();
    }

    void Update()
    {
        String currentType = this.layerData.type;

        if (currentType.Equals("start")) {
            original.SetActive(true);
            background.SetActive(false);
            layers.SetActive(false);

            ResetToOriginPositions();
        } else if (currentType.Equals("behind")) {
            original.SetActive(false);
            background.SetActive(true);
            layers.SetActive(true);
        } else if (currentType.Equals("before")) {
            original.SetActive(true);
            background.SetActive(false);
            layers.SetActive(false);
        } else if (currentType.Equals("section")) {
            original.SetActive(false);
            background.SetActive(true);
            layers.SetActive(true);
        }

        UpdateLayers();

        FocusCurrentLayer();
    }

    private void ResetToOriginPositions()
    {
        for (int i = 0; i < this.layers.transform.childCount; i++) {
            GameObject imageAnchor = this.layers.transform.GetChild(i).Find("ImageAnchor").gameObject;

            imageAnchor.transform.localRotation = Quaternion.Euler(0, 0, 0);
            imageAnchor.transform.position = Vector3.zero;
        }
    }

    private void UpdateLayers()
    {
        for (int i = 0; i < this.layers.transform.childCount; i++) {
            Layer layerData = this.content.accordion.layers[i];

            // if (layerData.type.Equals("section")) {
            //     // HighlightImageSection(i);
            // } else if (layerData.type.Equals("before")) {
            //     UpdateBeforePosition(i);
            // } else if (layerData.type.Equals("behind")) {
            //     UpdateSectionPosition(i);
            //     UpdateMaterial(i);
            // }

            UpdatePosition(i);
        }
    }

    private void UpdatePosition(int layerIndex)
    {
        GameObject imageAnchor = this.layers.transform.GetChild(layerIndex).Find("ImageAnchor").gameObject;

        float distance = GetDistance(step, layerIndex);

        if (towardsCamera) {
            moveTowardsCamera(imageAnchor, distance);
        } else {
            moveFromOrigin(imageAnchor, distance);
        }
    }

    private void UpdateBeforePosition(int layerIndex)
    {
        GameObject imageAnchor = this.layers.transform.GetChild(layerIndex).Find("ImageAnchor").gameObject;

        float distance = Mathf.Pow(step + this.layers.transform.childCount + 1, exponent) / Mathf.Pow(this.layers.transform.childCount, exponent);

        if (towardsCamera) {
            moveTowardsCamera(imageAnchor, distance);
        } else {
            moveFromOrigin(imageAnchor, distance);
        }
    }

    private float GetDistance(float step, int index)
    {
        // return Mathf.Pow(step + (this.sectionsCount - 1 - index), exponent) / Mathf.Pow(this.sectionsCount, exponent);

        if (index == 1 && step == 1) {

            Debug.Log("GetDistance: " + Mathf.Pow(step + (this.layers.transform.childCount - index), exponent) / Mathf.Pow(this.layers.transform.childCount, exponent));
        }

        return Mathf.Pow(step + (this.layers.transform.childCount - index), exponent) / Mathf.Pow(this.layers.transform.childCount, exponent);
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

    private void UpdateMaterial(int layerIndex)
    {
        Layer layerData = this.content.accordion.layers[layerIndex];
        GameObject imageAnchor = this.layers.transform.GetChild(layerIndex).Find("ImageAnchor").gameObject;

        if (layerData.type == "behind") {
            if (this.currentLayerIndex == layerIndex) {
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

    private void HighlightImageSection(int layerIndex)
    {
        GameObject imageAnchor = this.layers.transform.GetChild(layerIndex).Find("ImageAnchor").gameObject;

        float layerDistance = GetDistance(this.currentLayerIndex, layerIndex);

        if (layerDistance > this.currentLayerDistance) {
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

    private void FocusCurrentLayer()
    {
        if (this.currentLayerAnchor && this.step != 0 && this.step % 1 == 0) {
            float focusDistance = Vector3.Distance(Camera.main.transform.position, currentLayerAnchor.transform.position);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(focusDistance);
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

    public void OnUpdateStep(float step)
    {
        UpdateStep(step);
    }

    internal void SetStartOffset(int offset)
    {
        this.startOffset = offset;
    }

    public void Reset()
    {
        StopAllCoroutines();
        this.step = 0;
    }
}
