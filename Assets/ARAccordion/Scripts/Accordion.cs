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

    private int imageSectionsCount;
    private InfoFactory infoFactory;

    private float distanceFactor = 0.5f;

    private bool towardsCamera = true;

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
        var dummys = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "DummyObject");
        foreach (GameObject obj in dummys) obj.transform.gameObject.SetActive(false);

        this.imageSectionsCount = imageSections.transform.childCount;

        infoFactory = GetComponent<InfoFactory>();
    }

    void Start()
    {
        background.SetActive(false);

        this.distanceOfActiveTile = GetDistance(this.imageSectionsCount, 0);
    }

    internal void SetStart(int startLayer)
    {
        this.start = startLayer;
    }

    public void OnUpdateStep(float step)
    {
        if (this.step == step) {
            return;
        }

        this.step = step;

        if (step > this.imageSectionsCount) {
            ShowInfrared();
        } else if (step > 0) {
            ShowImageSections();
        } else if (step < 0) {
            ShowPainter();
        }

        HighlightImageSections();
    }

    void LateUpdate()
    {
        if (step > this.imageSectionsCount) {
            original.SetActive(false);
            background.SetActive(true);
            imageSections.SetActive(true);
            infrared.SetActive(true);
        } else if (step > 0) {
            original.SetActive(false);
            background.SetActive(true);
            imageSections.SetActive(true);
            infrared.SetActive(false);
        } else if (step < 0) {
            original.SetActive(true);
            background.SetActive(false);
            imageSections.SetActive(false);
            infrared.SetActive(false);
        } else if (step == 0) {
            original.SetActive(true);
            background.SetActive(false);
            imageSections.SetActive(false);
            infrared.SetActive(false);

            ResetToOriginPositions();
        }

        UpdateImageSectionPositions();
        UpdateInfraredPosition();
        UpdatePainterPosition();

        FocusActiveImage();
    }

    private void FocusActiveImage()
    {
        if (this.activeImage && this.step > 0 && this.step % 1 == 0) {
            float focusDistance = Vector3.Distance(Camera.main.transform.position, activeImage.transform.position);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(focusDistance);
        }
    }

    private void ResetToOriginPositions()
    {
        for (int i = 0; i < this.imageSectionsCount; i++) {
            GameObject image = this.imageSections.transform.GetChild(i).Find("Image").gameObject;

            image.transform.localRotation = Quaternion.Euler(0, 0, 0);
            image.transform.position = Vector3.zero;
        }
    }

    private void UpdateImageSectionPositions()
    {
        for (int i = 0; i < this.imageSectionsCount; i++) {
            GameObject image = this.imageSections.transform.GetChild(i).Find("Image").gameObject;

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
        float distance = Mathf.Pow(step + this.imageSectionsCount + 1, exponent) / Mathf.Pow(this.imageSectionsCount, exponent);

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
        return Mathf.Pow(step + index, exponent) / Mathf.Pow(this.imageSectionsCount, exponent);
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
        this.currentLayer = this.imageSectionsCount - Mathf.CeilToInt(step);
        this.activeImage = this.imageSections.transform.GetChild(currentLayer).Find("Image").gameObject;

        if (step % 1 == 0) {
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

    private void ShowInfrared()
    {
        this.activeImage = infrared;

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
        for (int i = 0; i < this.imageSectionsCount; i++) {
            GameObject image = this.imageSections.transform.GetChild(i).Find("Image").gameObject;

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

    internal void SetMoveTowardsCamera(bool towardsCamera)
    {
        this.towardsCamera = towardsCamera;
    }

    internal void SetContent(Content content)
    {
        this.content = content;
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
                OnUpdateStep(Mathf.Lerp(moveFrom, moveTo, progress));
                yield return new WaitForEndOfFrame();
            } else {
                if (moveTo > 0) OnUpdateStep(moveTo);
                isMoving = false;
                yield break;
            }
        }
    }
}
