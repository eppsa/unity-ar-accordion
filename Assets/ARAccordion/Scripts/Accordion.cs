using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.Rendering.PostProcessing;

public class Accordion : MonoBehaviour
{

    public GameObject infoPopup;

    [Header("Layer")]
    [SerializeField] GameObject[] tiles;

    [SerializeField] float factor = 1.0f;

    private int step = 0;

    void Update()
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

            tile.transform.localPosition = Vector3.MoveTowards(
                tile.transform.localPosition, 
                new Vector3(
                    tile.transform.localPosition.x, 
                    GetPositionY(step, i), 
                    tile.transform.localPosition.z), 
                0.5f * Time.deltaTime
            );    
        }
    }

    private float GetPositionY(int step, int index) {
        if (step == 0) {
            return 0.0001f * index + 0.0001f;
        }
        Debug.Log(Mathf.Pow((step + index) * factor, 3));
        return Mathf.Pow((step + index) * factor, 3);
    }

    private void Highlight() {
        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = tiles[i];
            Color color = tile.GetComponent<Renderer>().material.GetColor("_Color");

            tile.GetComponent<Renderer>().material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.5f));
        }

        if (step > 0) {
            GameObject activeTile = tiles[tiles.Length - step];
            Color activeTileColor = activeTile.GetComponent<Renderer>().material.GetColor("_Color");

            activeTile.GetComponent<Renderer>().material.SetColor("_Color", new Color(activeTileColor.r, activeTileColor.g, activeTileColor.b, 1.0f));
        }
    }
}
