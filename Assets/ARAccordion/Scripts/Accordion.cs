using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Model;
using System.Collections.Generic;
using System.Linq;
using System;

public class Accordion : MonoBehaviour
{
    [Header("Canvas")] [SerializeField] private InfoFactory infoFactory;

    [SerializeField] public GameObject mainCanvas;

    [SerializeField] private GameObject background;
    [SerializeField] private GameObject original;
    [SerializeField] private GameObject components;
    [SerializeField] private GameObject painter;

    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float exponent = 1;
    [SerializeField] private float moveDuration = 1.5f;

    [SerializeField] private Material defaultSpriteMaterial;
    [SerializeField] private Material dofSpriteMaterial;

    private float distanceFactor = 0.5f;

    private bool towardsCamera = true;

    private List<GameObject> images = new List<GameObject>();

    public float step = 0f;
    private int currentLayer = 0;
    private GameObject activeImage = null;

    private bool savedOrigins = false;

    private ARSessionOrigin sessionOrigin;

    private Vector3 activeTilePosition;

    private Content content;
    private int start = 0;

    public bool isMoving;

    public float Exponent { get => exponent; set => exponent = value; }
    public float DistanceFactor { get => distanceFactor; set => distanceFactor = value; }

    public GameObject ActiveImage { get => activeImage; }

    void OnEnable()
    {
        foreach (Transform component in components.transform) {
            images.Add(component.Find("Image").gameObject);
        }

        var dummys = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "DummyObject");
        foreach (GameObject obj in dummys) obj.transform.gameObject.SetActive(false);
    }

    void Start()
    {
        background.SetActive(false);
    }

    internal void SetStart(int startLayer)
    {
        this.start = startLayer;
    }

    public IEnumerator MoveToLayer(float moveTo)
    {
        isMoving = true;
        mainCanvas.SetActive(false);
        infoFactory.gameObject.SetActive(false);


        float moveFrom = step;

        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true) {
            currentDuration = Time.time - startTime;
            progress = currentDuration / moveDuration;

            if (progress <= 1.0f) {
                UpdateStep(Mathf.Lerp(moveFrom, moveTo, progress));
                yield return new WaitForEndOfFrame();
            } else {
                if (moveTo > 0) UpdateStep(moveTo);
                isMoving = false;
                mainCanvas.SetActive(true);
                yield break;
            }
        }
    }

    void LateUpdate()
    {
        if (step > 0) {
            original.SetActive(false);
            background.SetActive(true);
            components.SetActive(true);
            UpdatePositions();

            float focusDistance = Vector3.Distance(Camera.main.transform.position, images[images.Count - Mathf.CeilToInt(this.step)].transform.position);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(focusDistance);
        } else if (step < 0) {
            original.SetActive(true);
            background.SetActive(false);
            components.SetActive(false);

            UpdatePainterPosition();

            float focusDistance = Vector3.Distance(Camera.main.transform.position, painter.transform.position);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(focusDistance);
        } else {
            original.SetActive(true);
            background.SetActive(false);
            components.SetActive(false);

            UpdateToOriginPositions();
            UpdatePainterPosition();
        }
    }

    private void UpdateToOriginPositions()
    {
        for (int i = 0; i < images.Count; i++) {
            GameObject tile = images[i];

            tile.transform.localRotation = Quaternion.Euler(0, 0, 0);
            tile.transform.position = Vector3.zero;
        }
    }

    private void UpdatePositions()
    {
        for (int i = 0; i < images.Count; i++) {
            GameObject component = this.images[i];

            float distance = GetDistance(step, i);

            if (towardsCamera) {
                moveTowardsCamera(component, distance);
            } else {
                moveFromOrigin(component, distance);
            }
        }
    }

    private void UpdatePainterPosition()
    {
        float distance = Mathf.Pow(step + images.Count + 1, exponent) / Mathf.Pow(images.Count, exponent);

        if (towardsCamera) {
            moveTowardsCamera(painter, distance);
        } else {
            moveFromOrigin(painter, distance);
        }
    }

    private float GetDistance(float step, int index)
    {
        return Mathf.Pow(step + index, exponent) / Mathf.Pow(images.Count, exponent);
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
        Vector3 origin = go.transform.parent.transform.position;

        Vector3 distanceVector = Camera.main.transform.position - origin;

        Vector3 newPosition = origin + distanceVector * distanceFactor * stepDistance;

        go.transform.position = newPosition;

        if (Vector3.Distance(go.transform.position, origin) > 0.1f) {
            Vector3 newDirection = Vector3.RotateTowards(go.transform.forward, Camera.main.transform.forward, speed * 0.001f * Time.deltaTime, 0.0f);
            go.transform.rotation = Quaternion.LookRotation(newDirection, Camera.main.transform.up);
        } else {
            Vector3 newDirection = Vector3.RotateTowards(go.transform.forward, go.transform.parent.transform.forward, speed * 0.01f * Time.deltaTime, 0.0f);
            go.transform.rotation = Quaternion.LookRotation(newDirection, Camera.main.transform.up);
        }
    }

    internal void EnableInfoTags(bool enable)
    {
        infoFactory.transform.gameObject.SetActive(enable);
    }


    public void UpdateStep(float step)
    {
        if (this.step == step) {
            return;
        }

        this.step = step;

        if (this.step % 1 == 0) {
            Debug.Log("Step " + step);
        }

        if (step > 0) {
            ShowComponents();
        } else if (step < 0) {
            ShowPainter();
        }

        Highlight();
    }

    private void ShowPainter()
    {
        this.activeImage = painter;
        if (step % 1 == 0) {
            infoFactory.CreateInfoTag("Bla bla bla bla bla bla bla bla bla bla bla bla bla ...", activeImage.transform.Find("Anchors").GetChild(0));
        } else {
            Transform anchor = activeImage.transform.Find("Anchors").GetChild(0);
            if (anchor) {
                infoFactory.ClearInfoTag(anchor);
            }
        }
    }

    private void ShowComponents()
    {
        if (step % 1 == 0) {
            this.currentLayer = images.Count - Mathf.CeilToInt(step);
            this.activeImage = images[currentLayer];

            UpdateLayerUI();
        } else {
            if (activeImage != null) {
                Transform anchors = activeImage.transform.Find("Anchors");
                if (anchors) {
                    infoFactory.ClearInfoPoints(activeImage.transform.Find("Anchors"));
                }
            }
        }
    }

    private void UpdateLayerUI()
    {
        if (infoFactory.isActiveAndEnabled) {

            Transform anchors = activeImage.transform.Find("Anchors");

            if (anchors) {
                infoFactory.CreateInfoPoints(content.accordion.layers[this.currentLayer].infos, anchors, "Avatars/" + this.activeImage.transform.parent.name);
            }
        }
    }

    private void Highlight()
    {
        int activeTileIndex = images.Count - Mathf.CeilToInt(step);

        float distanceOfActiveTile = GetDistance(step, activeTileIndex);

        for (int i = 0; i < images.Count; i++) {
            GameObject component = images[i];
            Color color = component.GetComponentInChildren<Renderer>().material.GetColor("_Color");

            if (i == activeTileIndex) {
                component.GetComponentInChildren<Renderer>().material = dofSpriteMaterial;
                // infos.SetAnchor(tile.transform.Find("TagAnchor"));
            }

            float distanceOfTile = GetDistance(step, i);
            if (distanceOfTile > distanceOfActiveTile) {
                component.GetComponentInChildren<Renderer>().material = defaultSpriteMaterial;
                StartCoroutine(Fade(color.a, 0.5f, 1.0f, component.GetComponentInChildren<Renderer>().material));
            } else {
                component.GetComponentInChildren<Renderer>().material = dofSpriteMaterial;
                StartCoroutine(Fade(color.a, 1.0f, 1.0f, component.GetComponentInChildren<Renderer>().material));
            }
        }
    }

    private IEnumerator Fade(float fadeFrom, float fadeTo, float duration, Material material)
    {
        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        Color color = material.GetColor("_Color");

        while (true) {
            currentDuration = Time.time - startTime;
            progress = currentDuration / duration;

            if (progress <= 1.0f) {
                material.SetColor("_Color", new Color(color.r, color.g, color.b, Mathf.Lerp(fadeFrom, fadeTo, progress)));
                yield return new WaitForEndOfFrame();
            } else {
                material.SetColor("_Color", new Color(color.r, color.g, color.b, fadeTo));
                yield break;
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
}
