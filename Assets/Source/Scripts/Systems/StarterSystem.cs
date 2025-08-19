using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarterSystem : MonoBehaviour
{
    [SerializeField] string indexFingerTag;
    [SerializeField] LoadSavePictureSystem loadSaver;
    [SerializeField] PicturePaintingComponent picture;
    [SerializeField] UICanvasComponent UICanvas;
    [SerializeField] GameConfig gameConfig;

    private void Awake()
    {
        loadSaver.Prestart(picture);

        UICanvas.saveButton.onClick.AddListener(loadSaver.Save);
        UICanvas.loadButton.onClick.AddListener(loadSaver.Load);
        UICanvas.clearButton.onClick.AddListener(picture.Clear);

        UICanvas.colorImage.color = gameConfig.startBrushColor;

        UICanvas.palleteImage.Prestart(picture, UICanvas);
        picture.PreStart(indexFingerTag, loadSaver, gameConfig);

        UICanvas.brushSizeSlider.onValueChanged.AddListener(BrushSize);
        UICanvas.brushSizeSlider.value = gameConfig.startBrushSize;
        UICanvas.brushSizeText.text = gameConfig.startBrushSize.ToString("F0");
    }
    void BrushSize(float value)
    {
        picture.brushRadius = Mathf.RoundToInt(value);
        UICanvas.brushSizeText.text = UICanvas.brushSizeSlider.value.ToString("F0");
    }
    private void Update()
    {
        picture.Upd();
    }
}
