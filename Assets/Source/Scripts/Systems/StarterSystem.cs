using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarterSystem : MonoBehaviour
{
    [SerializeField] string indexFingerTag;
    [SerializeField] LoadSavePictureSystem loadSaver;
    [SerializeField] PicturePaintingComponent picture;
    [SerializeField] UICanvasComponent UICanvas;

    private void Awake()
    {
        loadSaver.Prestart(picture);

        UICanvas.saveButton.onClick.AddListener(loadSaver.Save);
        UICanvas.loadButton.onClick.AddListener(loadSaver.Load);
        UICanvas.clearButton.onClick.AddListener(picture.Clear);

        UICanvas.palleteImage.Prestart(picture, UICanvas);
        picture.PreStart(indexFingerTag);

        UICanvas.brushSizeSlider.onValueChanged.AddListener(BrushSize);
        UICanvas.brushSizeSlider.value = picture.brushThickness;
        UICanvas.brushSizeText.text = UICanvas.brushSizeSlider.value.ToString("F0");
    }
    void BrushSize(float value)
    {
        picture.brushThickness = value;
        UICanvas.brushSizeText.text = UICanvas.brushSizeSlider.value.ToString("F0");
    }
    private void Update()
    {
        picture.Upd();
    }
}
