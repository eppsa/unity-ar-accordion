using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Accordion : MonoBehaviour
{
    [Header ("Canvas")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private InfoPopup infoPopUp;

    [Header("Layer")]
    [SerializeField] GameObject[] tiles;

    private Vector3 initialCameraPosition;

    private int step = 0;
    private Vector3 activeTilePosition;

    private Vector3[] tilesOrigins;

    private bool moveTowardsCamera;

    private bool savedOrigins = false;

    private Dictionary<string, Dictionary<string, string>>  content;

    void Start()
    {    
        canvas.worldCamera = Camera.main;
        tilesOrigins = new Vector3[tiles.Length];

        infoPopUp.SetContent(content);
    }

    void LateUpdate()
    {
        Highlight();
        UpdatePositions();
    }

    public void UpdateStep(int step) {
        this.step = step;
    }

    private void UpdatePositions() {
        Debug.Log(Camera.main.transform.position);

        if (!savedOrigins) {
            for (int i = 0; i < tiles.Length; i++)
            {
                GameObject tile = tiles[i];
                tilesOrigins[i] = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);
                    
                Debug.Log("Accordion: Origin " + i + ": " + tilesOrigins[i]);
            }

            savedOrigins = true;
        }

        if (step > 0) {
            infoPopUp.transform.gameObject.SetActive(true);
            infoPopUp.SwitchLayer(step);
        } else {
            infoPopUp.gameObject.SetActive(false);
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = tiles[i];

            Vector3 newTarget;
            if (step == 0) {
                newTarget = tilesOrigins[i];
            } else if (moveTowardsCamera) {
                if (initialCameraPosition == null) {
                    this.initialCameraPosition = Camera.main.transform.position;
                }
                newTarget = tilesOrigins[i] + ((Camera.main.transform.position - tilesOrigins[i]) * GetDistance(step, i));
            } else {
                if (initialCameraPosition == null) {
                    this.initialCameraPosition = Camera.main.transform.position;
                }
                newTarget = tilesOrigins[i] + ((initialCameraPosition - tilesOrigins[i]) * GetDistance(step, i));
            }

            tile.transform.position = Vector3.MoveTowards(
                tile.transform.position,
                newTarget,
                1.0f * Time.deltaTime
            );
        }
    }

    private float GetDistance(int step, int index) {
        return Mathf.Pow(step + index, 3) / Mathf.Pow(tiles.Length + 1.0f, 3); 
    }

    private void Highlight() {
        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = tiles[i];
            Color color = tile.GetComponent<Renderer>().material.GetColor("_Color");

            tile.GetComponent<Renderer>().material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.3f));
        }

        if (step > 0) {
            GameObject activeTile = tiles[tiles.Length - step];
            Color activeTileColor = activeTile.GetComponent<Renderer>().material.GetColor("_Color");
            activeTile.GetComponent<Renderer>().material.SetColor("_Color", new Color(activeTileColor.r, activeTileColor.g, activeTileColor.b, 1.0f));
            
            canvas.transform.position = activeTile.transform.Find("TagAnchor").transform.position;

            // Update only z value of canvas
            //
            // canvasPrefab.transform.position = new Vector3 ( 
            //     canvasPrefab.transform.position.x,
            //     canvasPrefab.transform.position.y,
            //     accordionPrefab.GetComponent<Accordion>().activeTilePosition.z
            // )
        }
    }

    internal void SetMoveTowardsCamera(bool moveTowardsCamera)
    {
        this.moveTowardsCamera = moveTowardsCamera;
    }

    internal void SetContent(Dictionary<string, Dictionary<string, string>> content)
    {
        this.content = content;
    }
}
