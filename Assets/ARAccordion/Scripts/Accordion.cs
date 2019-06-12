using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.XR.ARFoundation;
using jsonObject;

public class Accordion : MonoBehaviour
{
    [Header ("Canvas")] [SerializeField] private InfoPopup infoPopUp;

    [SerializeField] private Quiz quiz;

    [Header("Layer")] [SerializeField] GameObject[] tiles;

    [SerializeField] float speed = 1.0f;

    [SerializeField] Material defaultSpriteMaterial;
    [SerializeField] Material dofSpriteMaterial;

    private int step = 0;

    private bool savedOrigins = false;

    private ARSessionOrigin sessionOrigin;

    private Vector3 initialCameraPosition;
    private Vector3 activeTilePosition;
    private Vector3[] tilesOrigins;

    private bool moveTowardsCamera = false;

    private Content content;
    

    void Start()
    {    
        infoPopUp.GetComponent<Canvas>().worldCamera = Camera.main;
        infoPopUp.SetFadeDuration(0.5f);

        quiz.GetComponent<Canvas>().worldCamera = Camera.main;

        Highlight();
    }

    void LateUpdate()
    {
        if (tilesOrigins != null) {
            UpdatePositions();
        }
    }

    public void UpdateStep(int step) {
        this.step = step;

        if (this.step > 0) {
            // Camera.main.GetComponentInChildren<PostProcessLayer>().enabled = true;   

            if (!savedOrigins) {
                tilesOrigins = new Vector3[tiles.Length];
                for (int i = 0; i < tiles.Length; i++)
                {
                    GameObject tile = tiles[i];
                    tilesOrigins[i] = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);
                }

                savedOrigins = true;
            }

            if (initialCameraPosition == null) {
                this.initialCameraPosition = Camera.main.transform.position;
            }

            GameObject activeTile = tiles[tiles.Length - step];

            if (infoPopUp.isActiveAndEnabled) {
                infoPopUp.SetAnchor(activeTile.transform.Find("TagAnchor"));
                infoPopUp.Show(tiles.Length - step);
            }

            if (quiz.isActiveAndEnabled) {
                quiz.transform.position = activeTile.transform.Find("TagAnchor").transform.position;
                quiz.transform.SetParent(activeTile.transform.Find("TagAnchor").transform);
            }
        } else {
            // Camera.main.GetComponentInChildren<PostProcessLayer>().enabled = false;

            if (infoPopUp.isActiveAndEnabled) {
               infoPopUp.Hide();
            }

            if (quiz.isActiveAndEnabled) {
                quiz.transform.gameObject.SetActive(false);
            }
        }

        Highlight();
    }

    private void UpdatePositions() {
        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = tiles[i];

            Vector3 newPosition;
            if (step == 0) {
                newPosition = tilesOrigins[i];
            } else if (moveTowardsCamera) {
                float distanceToCamera = Vector3.Distance(this.initialCameraPosition, tilesOrigins[0]);
                Vector3 targetPosition = new Vector3(this.initialCameraPosition.x, this.initialCameraPosition.y, this.initialCameraPosition.z + 0.5f * distanceToCamera);

                newPosition = tilesOrigins[i] + ((targetPosition - tilesOrigins[i]) * GetDistance(step, i));
                
                Vector3 newDir = Vector3.RotateTowards(tile.transform.forward, Camera.main.transform.forward, 0.3f * Time.deltaTime, 0.0f);
                tile.transform.rotation = Quaternion.LookRotation(newDir, Camera.main.transform.up);

                tile.transform.position = Vector3.MoveTowards(tile.transform.position, newPosition, 0.5f * Time.deltaTime);
            } else {
                float distanceToCamera = Vector3.Distance(tile.transform.InverseTransformPoint(this.initialCameraPosition), tile.transform.InverseTransformPoint(tilesOrigins[0]));

                Debug.Log("Distance to Camera: " + distanceToCamera);

                var newLocalPosition = tile.transform.InverseTransformPoint(tilesOrigins[i]) + (-Vector3.forward * GetDistance(step, i) * distanceToCamera * 0.5f);
                tile.transform.position = Vector3.MoveTowards(tile.transform.position, tile.transform.TransformPoint(newLocalPosition), 0.5f * Time.deltaTime);
            }
        }

        if (step > 0) {
            float distance = Vector3.Distance(Camera.main.transform.position, tiles[tiles.Length - step].transform.position);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(distance);
        }
    }

    private float GetDistance(int step, int index) {
        return Mathf.Pow(step + index, 4) / Mathf.Pow(tiles.Length, 4); 
    }

    private void Highlight() {
        if (step > 0) {
            for (int i = 0; i < tiles.Length; i++)
            {
                GameObject tile = tiles[i];
                Color color = tile.GetComponent<Renderer>().material.GetColor("_Color");
                tile.GetComponent<Renderer>().material = defaultSpriteMaterial;
                tile.GetComponent<Renderer>().material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.5f));
            }

            GameObject activeTile = tiles[tiles.Length - step];
            Color activeTileColor = activeTile.GetComponent<Renderer>().material.GetColor("_Color");
            activeTile.GetComponent<Renderer>().material = dofSpriteMaterial;
            activeTile.GetComponent<Renderer>().material.SetColor("_Color", new Color(activeTileColor.r, activeTileColor.g, activeTileColor.b, 1.0f));
            
            infoPopUp.SetFadeDuration(0.5f);
            infoPopUp.SetAnchor(activeTile.transform.Find("TagAnchor"));
        } else {
            for (int i = 0; i < tiles.Length; i++)
            {
                GameObject tile = tiles[i];
                Color color = tile.GetComponent<Renderer>().material.GetColor("_Color");
                tile.GetComponent<Renderer>().material = defaultSpriteMaterial;
                tile.GetComponent<Renderer>().material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.0f));
            }
        }
    }

    internal void SetMoveTowardsCamera(bool moveTowardsCamera)
    {
        this.moveTowardsCamera = moveTowardsCamera;
    }

    internal void ShowQuiz(bool show) {
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