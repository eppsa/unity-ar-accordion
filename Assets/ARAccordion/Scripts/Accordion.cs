using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Model;
using System.Collections.Generic;
using System.Linq;
using System;

public class Accordion : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject original;
    [SerializeField] private GameObject imageSections;
    [SerializeField] private GameObject painter;
    [SerializeField] private GameObject infrared;

    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float exponent = 1;
    [SerializeField] private float moveDuration = 1.5f;

    [SerializeField] private Material defaultSpriteMaterial;
    [SerializeField] private Material dofSpriteMaterial;

    private InfoFactory infoFactory;

    private float distanceFactor = 0.5f;

    private bool towardsCamera = true;

    private List<GameObject> images = new List<GameObject>();

    public float step = 0f;
    private int currentLayer = 0;
    private GameObject activeImage = null;
    private bool savedOrigins = false;

    private Vector3 activeTilePosition;
    private float distanceOfActiveTile;

    private Content content;
    private int start = 0;

    public bool isMoving;

    public float Exponent { get => exponent; set => exponent = value; }
    public float DistanceFactor { get => distanceFactor; set => distanceFactor = value; }

    public GameObject ActiveImage { get => activeImage; }

    void OnEnable()
    {
        foreach (Transform component in imageSections.transform) {
            images.Add(component.Find("Image").gameObject);
        }

        var dummys = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "DummyObject");
        foreach (GameObject obj in dummys) obj.transform.gameObject.SetActive(false);

        infoFactory = GetComponent<InfoFactory>();
    }

    void Start()
    {
        background.SetActive(false);

        this.distanceOfActiveTile = GetDistance(images.Count, 0);
        Debug.Log(distanceOfActiveTile);
    }

    internal void SetStart(int startLayer)
    {
        this.start = startLayer;
    }

    public IEnumerator MoveToLayer(float moveTo)
    {
        isMoving = true;

        infoFactory.enabled = false;

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
                yield break;
            }
        }
    }

    void LateUpdate()
    {
        if (step > images.Count) {
            original.SetActive(false);
            background.SetActive(true);
            imageSections.SetActive(true);

            UpdatePositions();
            UpdateInfraredPosition();

            float focusDistance = Vector3.Distance(Camera.main.transform.position, infrared.transform.position);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(focusDistance);
        } else if (step > 0) {
            original.SetActive(false);
            background.SetActive(true);
            imageSections.SetActive(true);

            UpdatePositions();
            UpdateInfraredPosition();
        } else if (step < 0) {
            original.SetActive(true);
            background.SetActive(false);
            imageSections.SetActive(false);

            UpdatePainterPosition();

            float focusDistance = Vector3.Distance(Camera.main.transform.position, painter.transform.position);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(focusDistance);
        } else if (step == 0) {
            original.SetActive(true);
            background.SetActive(false);
            imageSections.SetActive(false);

            UpdateToOriginPositions();
            UpdatePainterPosition();
            UpdateInfraredPosition();
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
            GameObject image = this.images[i];

            float distance = GetDistance(step, i);

            if (towardsCamera) {
                moveTowardsCamera(image, distance);
            } else {
                moveFromOrigin(image, distance);
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

    private void UpdateInfraredPosition()
    {
        float distance = GetDistance(step, -1);

        if (towardsCamera) {
            moveTowardsCamera(infrared, distance);
        } else {
            moveFromOrigin(infrared, distance);
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
        Vector3 originPosition = go.transform.parent.transform.position;

        Vector3 distanceVector = Camera.main.transform.position - originPosition;

        Vector3 newPosition = originPosition + distanceVector * distanceFactor * stepDistance;

        go.transform.position = newPosition;

        if (Vector3.Distance(newPosition, originPosition) > 0.1f) {
            Vector3 newDirection = Vector3.RotateTowards(go.transform.forward, Camera.main.transform.forward, speed * 0.001f * Time.deltaTime, 0.0f);
            go.transform.rotation = Quaternion.LookRotation(newDirection, Camera.main.transform.up);
        } else {
            Vector3 newDirection = Vector3.RotateTowards(go.transform.forward, go.transform.parent.transform.forward, speed * 0.01f * Time.deltaTime, 0.0f);
            go.transform.rotation = Quaternion.LookRotation(newDirection, Camera.main.transform.up);
        }
    }

    internal void EnableInfoTags(bool enable)
    {
        infoFactory.enabled = enable;
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

        if (step > images.Count) {
            ShowInfrared(true);
            HighlightImageSections();
        } else if (step > 0) {
            ShowInfrared(false);
            ShowImageSections();
            HighlightImageSections();
        } else if (step < 0) {
            ShowInfrared(false);
            ShowPainter();
        }
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

    private void ShowImageSections()
    {
        if (step % 1 == 0) {
            this.currentLayer = images.Count - Mathf.CeilToInt(step);
            this.activeImage = images[currentLayer];

            float focusDistance = Vector3.Distance(Camera.main.transform.position, activeImage.transform.position);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(focusDistance);

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

    private void ShowInfrared(bool show)
    {
        if (show) {
            infrared.SetActive(true);
            if (step % 1 != 0) {
                infrared.GetComponentInChildren<Renderer>().material = defaultSpriteMaterial;
                Material material = infrared.GetComponentInChildren<Renderer>().material;
                Color color = material.GetColor("_Color");

                material.SetColor("_Color", new Color(color.r, color.g, color.b, step % 1));
            } else {
                infrared.GetComponentInChildren<Renderer>().material = dofSpriteMaterial;
                Material material = infrared.GetComponentInChildren<Renderer>().material;
                Color color = material.GetColor("_Color");

                material.SetColor("_Color", new Color(color.r, color.g, color.b, 1));
            }
        } else {
            infrared.SetActive(false);
            infrared.GetComponentInChildren<Renderer>().material = defaultSpriteMaterial;
            Material material = infrared.GetComponentInChildren<Renderer>().material;
            Color color = material.GetColor("_Color");
            material.SetColor("_Color", new Color(color.r, color.g, color.b, 0));
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

    private void HighlightImageSections()
    {
        for (int i = 0; i < images.Count; i++) {
            GameObject image = images[i];

            float distanceOfTile = GetDistance(step, i);

            if (distanceOfTile > this.distanceOfActiveTile) {
                image.GetComponentInChildren<Renderer>().material = defaultSpriteMaterial;

                Material material = image.GetComponentInChildren<Renderer>().material;
                Color color = material.GetColor("_Color");
                material.SetColor("_Color", new Color(color.r, color.g, color.b, 1 - (step % 1)));
            } else {
                image.GetComponentInChildren<Renderer>().material = dofSpriteMaterial;

                Material material = image.GetComponentInChildren<Renderer>().material;
                Color color = material.GetColor("_Color");
                material.SetColor("_Color", new Color(color.r, color.g, color.b, 1));
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
