using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Accordion : MonoBehaviour
{
    public GameObject infoPopup;

    [Header("Layer")]
    [SerializeField] GameObject[] tiles;

    Transform target;

    private int step = 0;

    private Vector3[] tilesOrigins;

    void Start()
    {    
        tilesOrigins = new Vector3[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = tiles[i];
            tilesOrigins[i] = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);
                
            Debug.Log("Origin " + i + ": " + tilesOrigins[i]);
        }
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
        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = tiles[i];

            Vector3 newTarget;
            if (step == 0) {
                newTarget = tilesOrigins[i];
            } else {
                float distance = GetDistance(step, i);
                Debug.Log(distance);
                newTarget = tilesOrigins[i] + ((target.position - tilesOrigins[i]) * distance);
                // tile.transform.LookAt(newTarget);
                // tile.transform.Rotate(90, 0, 0);
            }

            tile.transform.position = Vector3.MoveTowards(
                tile.transform.position,
                newTarget,
                5.0f * Time.deltaTime);    
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
        }
    }

    public void SetTargetPosition(Transform target) {
        this.target = target;
        Debug.Log("targetPosition: " + target.position);
    }
}
