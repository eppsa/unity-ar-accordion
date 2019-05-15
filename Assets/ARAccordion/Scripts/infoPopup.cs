using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.Networking;


public class infoPopup : MonoBehaviour
{

 public GameObject Image;
 private string jsonString;

 public Dictionary<string, Dictionary <string, string>> layerConfig;
    

    void Start()
    {
        DeserializeJson();    
    }

    void DeserializeJson() {  
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "content.json");
        jsonString = File.ReadAllText(jsonPath);
        layerConfig = JsonConvert.DeserializeObject<Dictionary<string, Dictionary <string, string>>>(jsonString);
        UpdateInformation(0);
    }

    public void UpdateInformation(int layerNumber) {
        Text Information = this.transform.Find("Text").gameObject.GetComponent<Text>();
        Information.text = layerConfig["layer" + layerNumber]["information"];
        StartCoroutine (GetTexture (layerNumber));

    }

    IEnumerator GetTexture(int imageNumber) {
    {

    string url = Path.Combine(Application.streamingAssetsPath, "Icons/icon"+imageNumber+".jpg");

    byte[] imageData;
    Texture2D tex = new Texture2D(100, 100);

    //Check if we should use UnityWebRequest or File.ReadAllBytes
    if (url.Contains("://") || url.Contains(":///"))
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        imageData = www.downloadHandler.data;
    }
    else
    {
        imageData = File.ReadAllBytes(url);
    }
    Debug.Log(imageData.Length);

    //Load raw Data into Texture2D 
    tex.LoadImage(imageData);

    //Convert Texture2D to Sprite
    Vector2 pivot = new Vector2(0.5f, 0.5f);
    Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), pivot, 100.0f);

    Image.GetComponent<Image>().sprite = sprite;
}
}
}
