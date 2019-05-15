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
 private string jsonString;

 public Dictionary<string, Dictionary <string, string>> layerConfig;
    

    void Start()
    {
        DeserializeJson();
        UpdateInformation(0);
    }

    void DeserializeJson()
    {  
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "content.json");
        jsonString = File.ReadAllText(jsonPath);
        layerConfig = JsonConvert.DeserializeObject<Dictionary<string, Dictionary <string, string>>>(jsonString);
    }

    public void UpdateInformation(int layerNumber)
    {
        Text Information = this.transform.Find("Text").gameObject.GetComponent<Text>();
        Information.text = layerConfig["layer" + layerNumber]["information"];
        ChangeImage(layerNumber);
    }

    void ChangeImage(int layerNumber)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Icons/icon"+layerNumber+".jpg");

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
}
