using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalleteComponent : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Collider surfaceCollider;
    [SerializeField] private Renderer surfaceRenderer;

    [Header("Script")]
    [SerializeField] List<Transform> fingertipTransforms = new List<Transform>();

    [SerializeField] PicturePaintingComponent picture;

    string indexFingerTag;
    Texture2D textureCopy;
    Texture originalTexture;
    UICanvasComponent UICanvas;
    Transform pointer;
    public void PreStart(string indexFingerTag, PicturePaintingComponent picture, UICanvasComponent UICanvas)
    {
        pointer = new GameObject("pointer").transform;
        pointer.SetParent(transform);
        this.UICanvas = UICanvas;
        this.picture = picture;
        this.indexFingerTag = indexFingerTag;
        UICanvas.colorImage.color = picture.brushColor;

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
                GetColor();
        }
    }

    private void GetColor()
    {
        Vector3 pos = pointer.localPosition / 10f + Vector3.one / 2f;
        Vector2 uv = new Vector2(pos.x, pos.z);

        int texWidth = textureCopy.width;
        int texHeight = textureCopy.height;

        int x = Mathf.FloorToInt(texWidth - uv.x * texWidth);
        int y = Mathf.FloorToInt(texHeight - uv.y * texHeight);
        Debug.Log(pos + " " + uv + " " + x + " " + y);
        Color color = textureCopy.GetPixel(x, y);
        if (color.a > 0)
        {
            picture.brushColor = color;
            UICanvas.colorImage.color = color;
        }
    }
}
