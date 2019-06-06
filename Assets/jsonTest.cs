using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using jsonObject;
using Newtonsoft.Json;

public class jsonTest : MonoBehaviour
{
    //private Content content;
    void Start()
    {   
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "content.json");
        string jsonString = File.ReadAllText(jsonPath);

        Content content = JsonConvert.DeserializeObject<Content>(jsonString);
        Debug.Log(content.accordion.layers[0].quiz.questions[0].questionText);
    }
}
