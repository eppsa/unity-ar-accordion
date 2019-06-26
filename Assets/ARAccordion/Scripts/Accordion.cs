using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Model;

public class Accordion : MonoBehaviour
{
    [Header("Canvas")] [SerializeField] private InfoPopup infoPopUp;

    [SerializeField] private Quiz quiz;

    [Header("Layer")] [SerializeField] private GameObject[] tiles;

    [SerializeField] private GameObject background;

    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float distanceFactor = 0.5f;
    [SerializeField] private float exponent = 1;

    [SerializeField] private Material defaultSpriteMaterial;
    [SerializeField] private Material dofSpriteMaterial;

    [SerializeField] private bool moveTowardsCamera = false;

    private float distance = 0f;

    private bool savedOrigins = false;

    private ARSessionOrigin sessionOrigin;

    private Vector3 initialCameraPosition;
    private Vector3 activeTilePosition;
    private Vector3[] tilesOrigins;

    private Content content;


    void Start()
    {
        infoPopUp.GetComponent<Canvas>().worldCamera = Camera.main;
        infoPopUp.SetFadeDuration(0.5f);

        quiz.GetComponent<Canvas>().worldCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (tilesOrigins != null) {
            UpdatePositions();
        }
    }

    private void UpdatePositions()
    {
        for (int i = 0; i < tiles.Length; i++) {
            GameObject tile = tiles[i];

            if (moveTowardsCamera) {
                float distanceToCamera = Vector3.Distance(Camera.main.transform.position, tilesOrigins[0]);
                // Debug.Log("Distance to Camera: " + distanceToCamera);

                Vector3 targetPosition = Camera.main.transform.position + Camera.main.transform.forward * distanceFactor * distanceToCamera;

                Vector3 newPosition = tilesOrigins[i] + ((targetPosition - tilesOrigins[i]) * GetDistance(distance, i));
                tile.transform.position = Vector3.MoveTowards(tile.transform.position, newPosition, speed * Time.deltaTime);

                Vector3 newDirection = Vector3.RotateTowards(tile.transform.forward, Camera.main.transform.forward, speed * 0.5f * Time.deltaTime, 0.0f);
                tile.transform.rotation = Quaternion.LookRotation(newDirection, Camera.main.transform.up);
            } else {
                float distanceToCamera = Vector3.Distance(tile.transform.InverseTransformPoint(this.initialCameraPosition), tile.transform.InverseTransformPoint(tilesOrigins[0]));
                // Debug.Log("Local Distance to Camera: " + distanceToCamera);

                var newLocalPosition = tile.transform.InverseTransformPoint(tilesOrigins[i]) + (-Vector3.forward * GetDistance(distance, i) * distanceToCamera * distanceFactor);

                tile.transform.localRotation = Quaternion.Euler(0, 0, 0);
                tile.transform.position = Vector3.MoveTowards(tile.transform.position, tile.transform.TransformPoint(newLocalPosition), speed * Time.deltaTime);

                if (i == 7) {
                    Debug.Log("====================");
                    Debug.Log(newLocalPosition);
                    Debug.Log(distance);
                    Debug.Log(GetDistance(distance, i));
                    Debug.Log(tile.transform.TransformPoint(newLocalPosition));
                    Debug.Log(tile.transform.position);
                    Debug.Log("====================");
                }
            }
        }

        if (distance > 0) {
            float focusDistance = Vector3.Distance(Camera.main.transform.position, tiles[tiles.Length - Mathf.CeilToInt(this.distance)].transform.position);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(focusDistance);
        }
    }

    public void UpdateDistance(float distance)
    {
        this.distance = distance;

        if (!savedOrigins) {
            SaveOrigins();
            savedOrigins = true;
        }

        if (distance > 0) {
            if (distance % 1 == 0) {
                UpdateLayerUI();
            }

            background.SetActive(true);
        } else {
            if (infoPopUp.isActiveAndEnabled) {
                infoPopUp.Hide();
            }

            if (quiz.isActiveAndEnabled) {
                quiz.transform.gameObject.SetActive(false);
            }

            background.SetActive(false);
        }

        Highlight();
    }

    private void UpdateLayerUI()
    {
        if (distance == 0.0f) {
            return;
        }

        GameObject activeTile = tiles[tiles.Length - Mathf.CeilToInt(distance)];

        if (infoPopUp.isActiveAndEnabled) {
            infoPopUp.SetAnchor(activeTile.transform.Find("TagAnchor"));
            infoPopUp.Show(tiles.Length - Mathf.CeilToInt(distance));
        }

        if (quiz.isActiveAndEnabled) {
            quiz.transform.position = activeTile.transform.Find("TagAnchor").transform.position;
            quiz.transform.SetParent(activeTile.transform.Find("TagAnchor").transform);
        }
    }

    private void Highlight()
    {
        int activeTileIndex = tiles.Length - Mathf.CeilToInt(distance);

        float distanceOfActiveTile = GetDistance(distance, activeTileIndex);

        // Debug.Log("activeTileIndex " + activeTileIndex);
        // Debug.Log("activeTile: " + tiles[activeTileIndex]);
        // Debug.Log("distanceOfActiveTile " + distanceOfActiveTile);

        for (int i = 0; i < tiles.Length; i++) {
            GameObject tile = tiles[i];
            Color color = tile.GetComponent<Renderer>().material.GetColor("_Color");

            if (i == activeTileIndex) {
                tile.GetComponent<Renderer>().material = dofSpriteMaterial;
                infoPopUp.SetAnchor(tile.transform.Find("TagAnchor"));
            }

            float distanceOfTile = GetDistance(distance, i);
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
        // fadeRunning = true;

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
                // fadeRunning = false;
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

    private float GetDistance(float step, int index)
    {
        if (step == 0.0f) {
            return 0.0f;
        }
        return Mathf.Pow(step + index, exponent) / Mathf.Pow(tiles.Length, exponent);
    }

    internal void SetMoveTowardsCamera(bool moveTowardsCamera)
    {
        this.moveTowardsCamera = moveTowardsCamera;
    }

    internal void ShowQuiz(bool show)
    {
        this.quiz.transform.gameObject.SetActive(show);
        this.infoPopUp.transform.gameObject.SetActive(!show);

        UpdateDistance(this.distance);
    }

    internal void SetContent(Content content)
    {
        this.content = content;
        infoPopUp.SetContent(content);
        quiz.SetContent(content.accordion.layers[0].quiz);
    }
}
