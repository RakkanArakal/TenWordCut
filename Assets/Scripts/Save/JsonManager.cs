using System.Collections.Generic;
using UnityEngine;
using System.IO;


[System.Serializable]
public struct JsonData
{
    
    public int size ;

    public List<string> word;
    public List<string> sentence;
    
    
    public JsonData(int size)
    {
        this.size = size;
        this.word = new List<string>();
        this.sentence = new List<string>();
    }
}

public class JsonManager<DataType>
{
    public static DataType Read(string filePath)
    {
        string jsonText = File.ReadAllText(filePath);
        return JsonUtility.FromJson<DataType>(jsonText);
    }

    public static void Write(string filePath, DataType data)
    {
        string updatedJsonText = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, updatedJsonText);
    }
}
