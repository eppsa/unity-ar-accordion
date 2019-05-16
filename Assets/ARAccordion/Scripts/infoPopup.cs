using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.Networking;


public class infoPopup : MonoBehaviour
{

 public GameObject IconImage;
 public float fadeSpeed = 0.7f;
 private string jsonString;

 public Dictionary<string, Dictionary <string, string>> layerConfig;
    

    void Start()
    {
        DeserializeJson();
        SwitchLayer(0);

    }

    void DeserializeJson()
    {  
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "content.json");
        jsonString = File.ReadAllText(jsonPath);
        layerConfig = JsonConvert.DeserializeObject<Dictionary<string, Dictionary <string, string>>>(jsonString);
    }

    public void SwitchLayer(int newLayer)
    {
        StartCoroutine(Fade(1,0,fadeSpeed, newLayer));
    }

    void ChangeImage(int newLayer)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Icons/icon"+newLayer+".jpg");

        byte[] imageData;
        Texture2D Texture = new Texture2D(100, 100);
        imageData = File.ReadAllBytes(path);
        Texture.LoadImage(imageData);

        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Sprite sprite = Sprite.Create(Texture, new Rect(0.0f, 0.0f, Texture.width, Texture.height), pivot, 100.0f);

        IconImage.GetComponent<Image>().sprite = sprite;
    }

    public void TogglePopup()
    {
        if (this.gameObject.activeInHierarchy)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
        }
    }

    public IEnumerator Fade(float fadeFrom, float fadeTo, float lerpTime, int newLayer)
    {
        float startTime = Time.time;
        float currentTime = Time.time - startTime;
        float fadePercentage = currentTime / lerpTime;

        while(fadePercentage < 1) {

            currentTime = Time.time - startTime;
            fadePercentage = currentTime / lerpTime;

            float currentAlphaValue = Mathf.Lerp(fadeFrom, fadeTo, fadePercentage);

            this.GetComponent<CanvasGroup>().alpha = currentAlphaValue;

            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Fading done");

        if (this.GetComponent<CanvasGroup>().alpha == 0)
        {
            UpdateInformation(newLayer);
        }
    }

    void UpdateInformation(int newLayer)
    {
        Text Information = this.transform.Find("Text").gameObject.GetComponent<Text>();
        Information.text = layerConfig["layer" + newLayer]["information"];
        ChangeImage(newLayer);

        StartCoroutine(Fade(0,1,fadeSpeed,0));
    }
}