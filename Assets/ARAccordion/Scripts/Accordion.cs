using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Model;
using System;

public class Accordion : MonoBehaviour
{
    [Header("Canvas")] [SerializeField] private InfoPopup infoPopUp;

    [SerializeField] private Quiz quiz;

    [SerializeField] private GameObject background;
    [SerializeField] private GameObject original;
    [SerializeField] private GameObject componentAnchors;

    [SerializeField] private float speed = 5.0f;
    private float distanceFactor = 0.5f;
    [SerializeField] private float exponent = 1;

    [SerializeField] private Material defaultSpriteMaterial;
    [SerializeField] private Material dofSpriteMaterial;

    private bool towardsCamera = false;

    private GameObject[] components;

    private float step = 0f;

    private bool savedOrigins = false;

    private ARSessionOrigin sessionOrigin;

    private Vector3 initialCameraPosition;
    private Vector3 activeTilePosition;

    private Content content;

    public float Exponent { get => exponent; set => exponent = value; }

    void OnEnable()
    {
        components = new GameObject[componentAnchors.transform.childCount];
        for (int i = 0; i < componentAnchors.transform.childCount; i++) {
            components[i] = componentAnchors.transform.GetChild(i).Find("Image").gameObject;
        }

        if (Application.isEditor) {
            UpdateAnchors();
        }
    }

    void Start()
    {
        infoPopUp.gameObject.SetActive(true);
        infoPopUp.GetComponent<Canvas>().worldCamera = Camera.main;
        infoPopUp.SetFadeDuration(0.5f);

        background.SetActive(false);
    }

    void LateUpdate()
    {
        original.SetActive(step == 0);
        background.SetActive(step > 0);
        componentAnchors.SetActive(step > 0);

        if (step == 0) {
            SetOriginPositions();
        } else {
            SetNewPositions();
        }
    }

    private void SetOriginPositions()
    {
        for (int i = 0; i < components.Length; i++) {
            GameObject tile = components[i];

            tile.transform.localRotation = Quaternion.Euler(0, 0, 0);
            tile.transform.position = Vector3.zero;
        }
    }

    private void SetNewPositions()
    {
        for (int i = 0; i < components.Length; i++) {
            GameObject component = this.components[i];

            float distance = GetDistance(step, i);

            if (towardsCamera) {
                moveTowardsCamera(component, distance);
            } else {
                moveFromOrigin(component, distance);
            }
        }

        if (step > 0) {
            float focusDistance = Vector3.Distance(Camera.main.transform.position, components[components.Length - Mathf.CeilToInt(this.step)].transform.position);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(focusDistance);
        }
    }

    private float GetDistance(float step, int index)
    {
        return Mathf.Pow(step + index, exponent) / Mathf.Pow(components.Length, exponent);
    }

    private void moveFromOrigin(GameObject component, float stepDistance)
    {
        Vector3 origin = component.gameObject.transform.parent.transform.position;

        float distanceToCamera = Mathf.Abs(Vector3.Distance(this.initialCameraPosition, origin));


        Vector3 newLocalPosition = new Vector3(0, 0, -1) * stepDistance * distanceToCamera * distanceFactor;

        component.transform.localPosition = newLocalPosition;
    }

    private void moveTowardsCamera(GameObject component, float stepDistance)
    {
        Vector3 origin = component.gameObject.transform.parent.transform.position;

        Vector3 distanceVector = Camera.main.transform.position - origin;

        Vector3 newPosition = origin + distanceVector * distanceFactor * stepDistance;

        component.transform.position = newPosition;

        Vector3 newDirection = Vector3.RotateTowards(component.transform.forward, Camera.main.transform.forward, speed * 0.5f * Time.deltaTime, 0.0f);
        component.transform.rotation = Quaternion.LookRotation(newDirection, Camera.main.transform.up);
    }

    public void UpdateStep(float step)
    {
        this.step = step;

        if (step > 0) {
            if (step % 1 == 0) {
                UpdateLayerUI();
            }
        } else {
            if (infoPopUp.isActiveAndEnabled) {
                infoPopUp.Hide();
            }

            if (quiz.isActiveAndEnabled) {
                quiz.transform.gameObject.SetActive(false);
            }
        }

        Highlight();
    }

    private void UpdateLayerUI()
    {
        int layer = components.Length - Mathf.CeilToInt(step);
        GameObject activeTile = components[layer];

        if (infoPopUp.isActiveAndEnabled) {
            infoPopUp.SetAnchor(activeTile.transform.Find("TagAnchor"));
            infoPopUp.Show(content.accordion.layers[layer].information, "Images/icon" + layer);
        }

        if (quiz.isActiveAndEnabled) {
            quiz.transform.position = activeTile.transform.Find("TagAnchor").transform.position;
            quiz.transform.rotation = activeTile.transform.Find("TagAnchor").transform.rotation;
            quiz.transform.SetParent(activeTile.transform.Find("TagAnchor").transform);
        }
    }

    private void Highlight()
    {
        int activeTileIndex = components.Length - Mathf.CeilToInt(step);

        float distanceOfActiveTile = GetDistance(step, activeTileIndex);

        for (int i = 0; i < components.Length; i++) {
            GameObject tile = components[i];
            Color color = tile.GetComponent<Renderer>().material.GetColor("_Color");

            if (i == activeTileIndex) {
                tile.GetComponent<Renderer>().material = dofSpriteMaterial;
                infoPopUp.SetAnchor(tile.transform.Find("TagAnchor"));
            }

            float distanceOfTile = GetDistance(step, i);
            if (distanceOfTile > distanceOfActiveTile) {
                tile.GetComponent<Renderer>().material = defaultSpriteMaterial;
                StartCoroutine(Fade(color.a, 0.5f, 1.0f, tile.GetComponent<Renderer>().material));
            } else {
                tile.GetComponent<Renderer>().material = dofSpriteMaterial;
                StartCoroutine(Fade(color.a, 1.0f, 1.0f, tile.GetComponent<Renderer>().material));
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
        this.infoPopUp.gameObject.SetActive(!show);

        UpdateStep(this.step);
    }

    internal void SetContent(Content content)
    {
        this.content = content;
        quiz.SetContent(content.accordion.layers[0].quiz);
    }
}
