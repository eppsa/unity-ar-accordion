using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class getTexts : MonoBehaviour
{

 private string jsonString;

 public Dictionary<string, Dictionary <string, string>> layerConfig;
    

    void Start()
    {
        DeserializeJson();    
    }

    void DeserializeJson() {
        
        string jsonPath = Application.streamingAssetsPath + "/content.json";
        jsonString = File.ReadAllText(jsonPath);

        layerConfig = JsonConvert.DeserializeObject<Dictionary<string, Dictionary <string, string>>>(jsonString);

        Debug.Log(layerConfig["layer2"]["information"]);    
    }
}
