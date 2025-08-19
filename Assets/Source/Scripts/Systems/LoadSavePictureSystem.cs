using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class LoadSavePictureSystem : MonoBehaviour
{
    PicturePaintingComponent picture;
    [Header("Script")]
    [SerializeField] List<LineData> lines = new List<LineData>();
    bool isWorking;
    public void Prestart(PicturePaintingComponent picture)
    {
        this.picture = picture;
    }
    public void AddLine(LineData newLine)
    {
        lines.Add(newLine);
    }
    public void Save()
    {
        if (isWorking) return;
        isWorking = true;
     
        string filePath = Path.Combine(Application.persistentDataPath, "myPicture.json");
        DataWrapper wrapper = new DataWrapper(lines);
        File.WriteAllText(filePath, JsonUtility.ToJson(wrapper));
        Debug.Log("Picture saved to: " + filePath + " lines count:" + lines.Count);
        isWorking = false;
    }

    public void Load()
    {
        if (isWorking) return;
        isWorking = true;

        string filePath = Path.Combine(Application.persistentDataPath, "myPicture.json");
        if (!File.Exists(filePath))
        {
            Debug.Log("No save file");
            return;
        }
        string json = File.ReadAllText(filePath);
        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("File json is Empty");
            return;
        }
        DataWrapper data = JsonUtility.FromJson<DataWrapper>(json);
        if (data == null || data.lines.Count == 0)
        {
            Debug.Log("failed to parse JSON or data is null");
            return;
        }

        lines.Clear();
        foreach (var item in data.lines)
            lines.Add(item);
        picture.SetLoadedDatas(lines);
        Debug.Log("Picture loaded! Lines:" + lines.Count);

        isWorking = false;
    }

    [Serializable] class DataWrapper
    {
        public List<LineData> lines;

        public DataWrapper(List<LineData> lines)
        {
            this.lines = lines;
        }
    }
}