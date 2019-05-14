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
        string jsonPath = Application.streamingAssetsPath + "/content.json";
        jsonString = File.ReadAllText(jsonPath);

        layerConfig = JsonConvert.DeserializeObject<Dictionary<string, Dictionary <string, string>>>(jsonString);

        Debug.Log(layerConfig["layer2"]["information"]);
        
    }

    // void loadJsonFile() {
    //     string jsonPath = Application.dataPath + "/StreamingAssets/myData.Json";
    //     string jsonString = File.ReadAllText(jsonPath);
    //     JObject itemJson = JObject.Parse(jsonString);

    //     Debug.Log(itemJson);

    //      for (int i = 0; i < itemJson.Count; i++)
    //     {

    //         database.Add(i);
    //     }
    //     Debug.Log(database[1]);
    // }

}
