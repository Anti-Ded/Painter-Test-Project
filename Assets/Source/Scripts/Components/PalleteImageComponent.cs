using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PalleteImageComponent : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image image;

    PicturePaintingComponent picture;
    UICanvasComponent UICanvas;

    public void Prestart(PicturePaintingComponent picture, UICanvasComponent UICanvas)
    {
        this.UICanvas = UICanvas;
        this.picture = picture;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Texture2D texture = image.sprite.texture;
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(image.rectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            // Adjust localPoint to be relative to the textureRect's origin
            localPoint.x += image.rectTransform.sizeDelta.x / 2f;
            localPoint.y += image.rectTransform.sizeDelta.y / 2f;

            // Calculate pixel coordinates within the texture
            int px = Mathf.FloorToInt(localPoint.x * (texture.width / image.rectTransform.sizeDelta.x));
            int py = Mathf.FloorToInt(localPoint.y * (texture.height / image.rectTransform.sizeDelta.y));

            Color pixelColor = texture.GetPixel(px, py);
            if (pixelColor.a > 0)
            {
                picture.brushColor = pixelColor;
                UICanvas.colorImage.color = pixelColor;
            }
        }
    }
}
