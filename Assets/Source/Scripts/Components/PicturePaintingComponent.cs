using UnityEngine;
using System.Collections.Generic;

public class PicturePaintingComponent : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Collider surfaceCollider;
    [SerializeField] private Renderer surfaceRenderer;

    [Header("Brush Settings")]
    [Tooltip("Color used for drawing on the texture.")]
    public Color brushColor = Color.red;

    [Tooltip("Brush thickness in pixels.")]
    public float brushThickness = 8f;

    [Header("Script")]
    [SerializeField] List<Transform> fingertipTransforms = new List<Transform>();

    string indexFingerTag;
    Texture2D textureCopy;
    Texture originalTexture;
    Transform pointer;

    public void PreStart(string indexFingerTag)
    {
        pointer = new GameObject("pointer").transform;
        pointer.SetParent(transform);

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
        if (other.gameObject.tag == indexFingerTag && !fingertipTransforms.Contains(other.transform))
            fingertipTransforms.Add(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == indexFingerTag && fingertipTransforms.Contains(other.transform))
            fingertipTransforms.Remove(other.transform);
    }
    public void Upd()
    {
        if (textureCopy == null || fingertipTransforms == null)
            return;

        foreach (var fingertip in fingertipTransforms)
        {
            pointer.position = fingertip.position;
            pointer.localPosition = new Vector3(pointer.localPosition.x, 0.1f, pointer.localPosition.z);
            Ray ray = new Ray(pointer.position, -transform.up);
            if (Physics.Raycast(ray, out RaycastHit hit, 0.5f))
                PaintAt();
        }
    }

    /// <summary>
    /// Paints a filled circle on textureCopy at the hit UV coordinate.
    /// </summary>
    /// <param name="hit">RaycastHit containing textureCoord (Vector2 uv).</param>
    private void PaintAt()
    {
        Vector3 pos = pointer.localPosition / 10f + Vector3.one / 2f;
        Vector2 uv = new Vector2(pos.x, pos.z);

        int texWidth = textureCopy.width;
        int texHeight = textureCopy.height;

        int x = Mathf.FloorToInt(texWidth - uv.x * texWidth);
        int y = Mathf.FloorToInt(texHeight - uv.y * texHeight);
        int radius = Mathf.CeilToInt(brushThickness);

        // Loop over a square region and set pixels inside the circle
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy <= radius * radius)
                {
                    int px = x + dx;
                    int py = y + dy;
                    if (px >= 0 && px < texWidth && py >= 0 && py < texHeight)
                    {
                        textureCopy.SetPixel(px, py, brushColor);
                    }
                }
            }
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

    void OnDestroy()
    {
        // Restore the original texture to avoid leaking our clone
        if (surfaceRenderer != null && originalTexture != null)
        {
            surfaceRenderer.material.mainTexture = originalTexture;
        }
    }
}