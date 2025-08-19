using UnityEngine;
using System;
using System.IO;

public class LoadSavePictureSystem : MonoBehaviour
{
    PicturePaintingComponent picture;
    
    public void Prestart(PicturePaintingComponent picture)
    {
        this.picture = picture;
    }

    public void Save()
    {
        Texture2D texture = picture.GetTexture();
        if (texture == null)
            Debug.LogError("Texture2DJsonConverter.Serialize: texture is null");

        TextureData data = new TextureData
        {
            width = texture.width,
            height = texture.height,
            format = texture.format
        };

        Color[] colors = texture.GetPixels();
        data.pixels = new Pixel[colors.Length];
        for (int i = 0; i < colors.Length; i++)
            data.pixels[i] = new Pixel(colors[i]);

        string filePath = Path.Combine(Application.persistentDataPath, "myPicture.json");
        File.WriteAllText(filePath, JsonUtility.ToJson(data));
        Debug.Log("Picture saved to: " + filePath);
    }

    public void Load()
    {
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
        TextureData data = JsonUtility.FromJson<TextureData>(json);
        if (data == null)
        {
            Debug.LogError("Texture2DJsonConverter.Deserialize: failed to parse JSON");
            return;
        }

        Texture2D texture = new Texture2D(data.width, data.height, data.format, false);
        Color[] colors = new Color[data.pixels.Length];
        for (int i = 0; i < data.pixels.Length; i++)
        {
            colors[i] = data.pixels[i].ToColor();
        }

        texture.SetPixels(colors);
        texture.Apply();
        picture.SetTexture(texture);
        Debug.Log("Picture loaded");
    }

    // Serializable pixel data (RGBA) for JSON conversion
    [Serializable]
    private struct Pixel
    {
        public float r, g, b, a;
        public Pixel(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }
        public Color ToColor()
        {
            return new Color(r, g, b, a);
        }
    }

    // Container for texture metadata and pixel array
    [Serializable]
    private class TextureData
    {
        public int width;
        public int height;
        public TextureFormat format;
        public Pixel[] pixels;
    }
}