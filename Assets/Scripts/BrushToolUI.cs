using UnityEngine;
using UnityEngine.UI;

public class BrushToolUI : MonoBehaviour
{
    public DrawManager manager;
    public Button brushButton;
    public GameObject judgingScreen;
    public UploadDownloadDrawing uploader;

    void Start()
    {
        UpdateBrushButtonColour();
    }

    // called by the brushSlider
    public void SetBrushSize(float brushSize)
    {
        manager.SetBrushSize(brushSize);
    }

    private void OnDisable()
    {
        Clear();
    }

    // called when the colour picker button is pressed
    public void SetBrushColour()
    {
        // update the colour of the draw prefab
        manager.UpdateBrushColour();

        // change the brush button colour
        UpdateBrushButtonColour();
    }

    public void UpdateBrushButtonColour()
    {
        Color brushColour = manager.GetBrushColour();
        ColorBlock buttonColour = brushButton.colors;
        buttonColour.normalColor = brushColour;
        buttonColour.highlightedColor = brushColour;
        buttonColour.selectedColor = brushColour;
        brushButton.colors = buttonColour;
    }

    // called when the eraser button is pressed
    public void SwitchToEraser()
    {
        manager.SetBrushColour(Color.white);
    }

    public void SwitchToBrush()
    {
        manager.SetBrushColour(brushButton.colors.normalColor);
    }

    public void Clear()
    {
        manager.Clear();
    }

    

}
