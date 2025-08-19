using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;

public class PicturePaintingComponent : MonoBehaviour
{
    [SerializeField] float drawDistance = 0.01f;
    [SerializeField] int drawMaxCount = 16;
    [Header("Components")]
    [SerializeField] private Collider surfaceCollider;
    [SerializeField] private Renderer surfaceRenderer;

    [Header("Brush Settings")]
    [Tooltip("Color used for drawing on the texture.")]
    public Color brushColor = Color.red;

    [Tooltip("Brush thickness in pixels.")]
    public int brushRadius = 8;

    string indexFingerTag;
    Texture2D textureCopy;
    Texture originalTexture;
    Transform pointer;
    LoadSavePictureSystem loadSaver;

    [Header("Script")]
    [SerializeField] SerializedDictionary<Transform, LineData> dictFingerLineData = new SerializedDictionary<Transform, LineData>();
    [SerializeField] List<DrawData> drawDatas = new List<DrawData>();

    public void PreStart(string indexFingerTag, LoadSavePictureSystem loadSaver)
    {
        pointer = new GameObject("pointer").transform;
        pointer.SetParent(transform);

        this.loadSaver = loadSaver;
        this.indexFingerTag = indexFingerTag;
        if (surfaceRenderer == null)
        {
            Debug.LogError("VRFingerPainter: No Renderer found on GameObject.");
            enabled = false;
            return;
        }

        if (surfaceCollider == null)
        {
            Debug.LogError("VRFingerPainter: No Collider found on GameObject.");
            enabled = false;
            return;
        }

        // Determine the source texture
        originalTexture = surfaceRenderer.material.mainTexture;
        if (originalTexture == null)
        {
            Debug.LogError("VRFingerPainter: Renderer has no mainTexture to draw on.");
            enabled = false;
            return;
        }

        // Clone the texture so we don’t overwrite the original asset
        Texture2D srcTex = originalTexture as Texture2D;
        if (srcTex == null)
        {
            Debug.LogError("VRFingerPainter: The texture must be a Texture2D.");
            enabled = false;
            return;
        }

        textureCopy = Instantiate(srcTex);
        textureCopy.name = srcTex.name + "_FingerPaintClone";

        // Assign the clone to the material
        surfaceRenderer.material.mainTexture = textureCopy;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == indexFingerTag && !dictFingerLineData.ContainsKey(other.transform))
        {
            // new Line Data
            LineData newLineData = new LineData();
            newLineData.startPoint = GetUVCoordinates(other.transform);

            // Add line data and waiting for TriggerExit
            dictFingerLineData.Add(other.transform, newLineData);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == indexFingerTag && dictFingerLineData.ContainsKey(other.transform))
        {
            LineData lineData = dictFingerLineData[other.transform];
            // Set Line datas
            lineData.radius = brushRadius;
            lineData.color = brushColor;
            lineData.endPoint = GetUVCoordinates(other.transform);
            // Add LineData to Saver
            loadSaver.AddLine(lineData);
            dictFingerLineData.Remove(other.transform);

            // Add DrawDatas
            float distance = Vector2Int.Distance(lineData.endPoint, lineData.startPoint);
            for (int i = 0; i < distance / drawDistance; i++)
            {
                DrawData newDraw = new DrawData();
                int x = Mathf.RoundToInt(Mathf.Lerp(lineData.endPoint.x, lineData.startPoint.x, i / distance));
                int y = Mathf.RoundToInt(Mathf.Lerp(lineData.endPoint.y, lineData.startPoint.y, i / distance));
                newDraw.point = new Vector2Int(x, y);
                newDraw.color = brushColor;
                newDraw.radius = brushRadius;
                drawDatas.Add(newDraw);
            }
        }
    }

    Vector2Int GetUVCoordinates(Transform target)
    {
        // Calculating of Start Line position
        pointer.position = target.transform.position;
        pointer.localPosition = new Vector3(pointer.localPosition.x, 0.1f, pointer.localPosition.z);

        // UV position
        Vector3 pos = pointer.localPosition / 10f + Vector3.one / 2f;
        Vector2 uv = new Vector2(pos.x, pos.z); // Via picture rotation

        // Texture coordinates
        int x = Mathf.FloorToInt(textureCopy.width - uv.x * textureCopy.width); // Reverse textures picture
        int y = Mathf.FloorToInt(textureCopy.height - uv.y * textureCopy.height);

        return new Vector2Int(x, y);
    }
    public void Upd()
    {
        // Draw DrawDatas to picture
        if (drawDatas.Count > 0)
            for (int i = 0; i < Mathf.Min(drawMaxCount, drawDatas.Count); i++) // max draw count to prevent lags
            {
                Paint(drawDatas[drawDatas.Count - 1]);
                drawDatas.RemoveAt(drawDatas.Count - 1);
            }
        /*   if (textureCopy == null || fingertipTransforms == null)
               return;

           foreach (var fingertip in fingertipTransforms)
           {
               pointer.position = fingertip.position;
               pointer.localPosition = new Vector3(pointer.localPosition.x, 0.1f, pointer.localPosition.z);
               Ray ray = new Ray(pointer.position, -transform.up);
               if (Physics.Raycast(ray, out RaycastHit hit, 0.5f))
                   PaintAt();
           }*/
    }

    /// <summary>
    /// Paints a filled circle on textureCopy at the hit UV coordinate.
    /// </summary>
    /// <param name="hit">RaycastHit containing textureCoord (Vector2 uv).</param>
    private void Paint(DrawData drawData)
    {
        Vector2Int uv = drawData.point;

        int texWidth = textureCopy.width;
        int texHeight = textureCopy.height;

        int x = texWidth - uv.x * texWidth;
        int y = texHeight - uv.y * texHeight;
        int radius = drawData.radius;

        // Loop over a square region and set pixels inside the circle
        for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
                if (dx * dx + dy * dy <= radius * radius)
                {
                    int px = x + dx;
                    int py = y + dy;
                    if (px >= 0 && px < texWidth && py >= 0 && py < texHeight)
                        textureCopy.SetPixel(px, py, drawData.color);
                }

        textureCopy.Apply();
    }
    public void SetTexture(Texture2D texture)
    {
        textureCopy = Instantiate(texture);
        textureCopy.Apply();
        surfaceRenderer.material.mainTexture = textureCopy;
    }
    public Texture2D GetTexture()
    {
        return textureCopy;
    }
    public void Clear()
    {
        if (textureCopy == null)
            return;

        int width = textureCopy.width;
        int height = textureCopy.height;
        Color[] fillColors = new Color[width * height];

        for (int i = 0; i < fillColors.Length; i++)
        {
            fillColors[i] = Color.white;
        }

        textureCopy.SetPixels(fillColors);
        textureCopy.Apply();
        Debug.Log("Picture cleared");
    }

    [Serializable] class DrawData
    {
        public Vector2Int point;
        public int radius;
        public Color color;
    }
}