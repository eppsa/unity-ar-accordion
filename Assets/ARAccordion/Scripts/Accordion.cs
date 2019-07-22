using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Model;
using System;
using System.Collections.Generic;

public class Accordion : MonoBehaviour
{
    [Header("Canvas")] [SerializeField] private InfoFactory infoFactory;

    [SerializeField] private Quiz quiz;

    [SerializeField] private GameObject background;
    [SerializeField] private GameObject original;
    [SerializeField] private GameObject components;
    [SerializeField] private GameObject painter;

    [SerializeField] private float speed = 5.0f;
    private float distanceFactor = 0.5f;
    [SerializeField] private float exponent = 1;

    [SerializeField] private Material defaultSpriteMaterial;
    [SerializeField] private Material dofSpriteMaterial;


    private bool towardsCamera = true;

    private List<GameObject> images = new List<GameObject>();

    private float step = 0f;
    private int currentLayer = 0;
    private GameObject activeImage = null;

    private bool savedOrigins = false;

    private ARSessionOrigin sessionOrigin;

    private Vector3 initialCameraPosition;
    private Vector3 activeTilePosition;

    private Content content;
    private int start = 0;

    public float Exponent { get => exponent; set => exponent = value; }

    void OnEnable()
    {
        foreach (Transform component in components.transform) {
            images.Add(component.Find("Image").gameObject);
        }

        if (Application.isEditor) {
            UpdateAnchors();
        }
    }

    void Start()
    {
        infoFactory.gameObject.SetActive(true);
        infoFactory.GetComponent<Canvas>().worldCamera = Camera.main;

        background.SetActive(false);
    }

    internal void SetStart(int startLayer)
    {
        this.start = startLayer;
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

        float distanceToCamera = Mathf.Abs(Vector3.Distance(this.initialCameraPosition, origin));

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
        } else {
            this.currentLayer = 0;
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

        if (quiz.isActiveAndEnabled) {
            quiz.transform.position = activeImage.transform.Find("TagAnchor").transform.position;
            quiz.transform.rotation = activeImage.transform.Find("TagAnchor").transform.rotation;
            quiz.transform.SetParent(activeImage.transform.Find("TagAnchor").transform);
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

    public void UpdateAnchors()
    {
        if (initialCameraPosition == null) {
            this.initialCameraPosition = Camera.main.transform.position;
        }
    }

    internal void SetMoveTowardsCamera(bool towardsCamera)
    {
        this.towardsCamera = towardsCamera;
    }

    internal void ShowQuiz(bool show)
    {
        this.quiz.gameObject.SetActive(show);
        this.infoFactory.gameObject.SetActive(!show);

        UpdateStep(this.step);
    }

    internal void SetContent(Content content)
    {
        this.content = content;
        quiz.SetContent(content.accordion.layers[2].quiz);
    }
}
