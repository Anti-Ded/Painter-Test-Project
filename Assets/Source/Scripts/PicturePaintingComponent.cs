using UnityEngine;
using System.Collections.Generic;

public class PicturePaintingComponent : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Collider surfaceCollider;
    [SerializeField] private Renderer surfaceRenderer;

    [Header("References")]
    [Tooltip("Transforms of the VR fingertip(s) used for painting.")]
    public List<Transform> fingertipTransforms = new List<Transform>();

    [Tooltip("Optional: supply a texture to draw on. If null, uses the Renderer’s mainTexture.")]
    public Texture2D drawableTexture;

    [Header("Brush Settings")]
    [Tooltip("Color used for drawing on the texture.")]
    public Color brushColor = Color.red;

    [Tooltip("Brush thickness in pixels.")]
    public float brushThickness = 8f;

    private Texture2D textureCopy;
    private Texture originalTexture;

    void Start()
    {
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
        if (drawableTexture != null)
        {
            originalTexture = drawableTexture;
        }
        else
        {
            originalTexture = surfaceRenderer.material.mainTexture;
            if (originalTexture == null)
            {
                Debug.LogError("VRFingerPainter: Renderer has no mainTexture to draw on.");
                enabled = false;
                return;
            }
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

    void Update()
    {
        if (textureCopy == null || fingertipTransforms == null)
            return;

        foreach (var fingertip in fingertipTransforms)
        {
            Ray ray = new Ray(fingertip.position, -transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 0.01f))
                PaintAt(hit);
        }
    }

    /// <summary>
    /// Paints a filled circle on textureCopy at the hit UV coordinate.
    /// </summary>
    /// <param name="hit">RaycastHit containing textureCoord (Vector2 uv).</param>
    private void PaintAt(RaycastHit hit)
    {
        Debug.Log(hit.point);
        Vector2 uv = hit.textureCoord;
        int texWidth = textureCopy.width;
        int texHeight = textureCopy.height;

        int xCenter = Mathf.FloorToInt(uv.x * texWidth);
        int yCenter = Mathf.FloorToInt(uv.y * texHeight);

        int radius = Mathf.CeilToInt(brushThickness);

        // Loop over a square region and set pixels inside the circle
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy <= radius * radius)
                {
                    int px = xCenter + dx;
                    int py = yCenter + dy;
                    if (px >= 0 && px < texWidth && py >= 0 && py < texHeight)
                    {
                        textureCopy.SetPixel(px, py, brushColor);
                    }
                }
            }
        }
        textureCopy.Apply();
    }

    /// <summary>
    /// Clears the entire painting canvas by filling it with white.
    /// </summary>
    public void ClearCanvas()
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