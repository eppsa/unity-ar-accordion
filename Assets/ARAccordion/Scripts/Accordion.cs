using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Model;
using System;

public class Accordion : MonoBehaviour
{
    [Header("Canvas")] [SerializeField] private InfoPopup infoPopUp;

    [SerializeField] private Quiz quiz;

    [Header("Layer")] [SerializeField] private GameObject[] tiles;

    [SerializeField] private GameObject background;
    [SerializeField] private GameObject original;
    [SerializeField] private GameObject components;

    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float distanceFactor = 0.5f;
    [SerializeField] private float exponent = 1;

    [SerializeField] private Material defaultSpriteMaterial;
    [SerializeField] private Material dofSpriteMaterial;

    [SerializeField] private bool towardsCamera = false;

    private float step = 0f;

    private bool savedOrigins = false;

    private ARSessionOrigin sessionOrigin;

    private Vector3 initialCameraPosition;
    private Vector3 activeTilePosition;
    private Vector3[] tilesOrigins;

    private Content content;

    public float Exponent { get => exponent; set => exponent = value; }

    void Start()
    {
        infoPopUp.GetComponent<Canvas>().worldCamera = Camera.main;
        infoPopUp.SetFadeDuration(0.5f);

        quiz.GetComponent<Canvas>().worldCamera = Camera.main;

        background.SetActive(false);
    }

    void LateUpdate()
    {
        original.SetActive(step == 0);
        background.SetActive(step > 0);
        components.SetActive(step > 0);

        if (tilesOrigins != null) {
            UpdatePositions();
        }
    }

    private void UpdatePositions()
    {
        for (int i = 0; i < tiles.Length; i++) {
            GameObject tile = tiles[i];

            float distance = GetDistance(step, i);

            if (i == 7) {
                Debug.Log("Distance: " + distance);
            }

            if (towardsCamera) {
                moveTowardsCamera(tile, tilesOrigins[i], distance);
            } else {
                moveFromOrigin(tile, tilesOrigins[i], distance);
            }
        }

        if (step > 0) {
            float focusDistance = Vector3.Distance(Camera.main.transform.position, tiles[tiles.Length - Mathf.CeilToInt(this.step)].transform.position);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(focusDistance);
        }
    }

    private float GetDistance(float step, int index)
    {
        float weight = step < 1 ? step : 1;
        return Mathf.Pow(step + index * weight, exponent) / Mathf.Pow(tiles.Length, exponent);
    }

    private void moveFromOrigin(GameObject tile, Vector3 origin, float distance)
    {
        float distanceToCamera = Vector3.Distance(this.initialCameraPosition, origin);

        Vector3 newPosition = origin + (-tile.transform.forward * distance * distanceToCamera * distanceFactor);

        tile.transform.localRotation = Quaternion.Euler(0, 0, 0);
        tile.transform.position = newPosition;
    }

    private void moveTowardsCamera(GameObject tile, Vector3 origin, float distance)
    {
        float distanceToCamera = Vector3.Distance(Camera.main.transform.position, origin);

        Vector3 targetPosition = Camera.main.transform.position + Camera.main.transform.forward * distanceFactor * distanceToCamera;
        Vector3 newPosition = origin + ((targetPosition - origin) * distance);
        tile.transform.position = Vector3.MoveTowards(tile.transform.position, newPosition, speed * Time.deltaTime);

        Vector3 newDirection = Vector3.RotateTowards(tile.transform.forward, Camera.main.transform.forward, speed * 0.5f * Time.deltaTime, 0.0f);
        tile.transform.rotation = Quaternion.LookRotation(newDirection, Camera.main.transform.up);
    }

    public void UpdateStep(float step)
    {
        this.step = step;

        if (!savedOrigins) {
            SaveOrigins();
            savedOrigins = true;
        }

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
        if (step == 0.0f) {
            return;
        }

        GameObject activeTile = tiles[tiles.Length - Mathf.CeilToInt(step)];

        if (infoPopUp.isActiveAndEnabled) {
            infoPopUp.SetAnchor(activeTile.transform.Find("TagAnchor"));
            infoPopUp.Show(tiles.Length - Mathf.CeilToInt(step));
        }

        if (quiz.isActiveAndEnabled) {
            quiz.transform.position = activeTile.transform.Find("TagAnchor").transform.position;
            quiz.transform.SetParent(activeTile.transform.Find("TagAnchor").transform);
        }
    }

    private void Highlight()
    {
        int activeTileIndex = tiles.Length - Mathf.CeilToInt(step);

        float distanceOfActiveTile = GetDistance(step, activeTileIndex);

        for (int i = 0; i < tiles.Length; i++) {
            GameObject tile = tiles[i];
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

    private void SaveOrigins()
    {
        tilesOrigins = new Vector3[tiles.Length];
        for (int i = 0; i < tiles.Length; i++) {
            GameObject tile = tiles[i];
            tilesOrigins[i] = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);
        }

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
        this.quiz.transform.gameObject.SetActive(show);
        this.infoPopUp.transform.gameObject.SetActive(!show);

        UpdateStep(this.step);
    }

    internal void SetContent(Content content)
    {
        this.content = content;
        infoPopUp.SetContent(content);
        quiz.SetContent(content.accordion.layers[0].quiz);
    }
}
