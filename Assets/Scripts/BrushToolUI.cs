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
        buttonColour.pressedColor = brushColour; // can make it a bit darker?
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

    public void OnSubmit()
    {
        // upload
        uploader.Save();

        // save the url in player info

        // load judging only when everyone has submitted so this part should be moved
        judgingScreen.SetActive(true);
        this.gameObject.SetActive(false);
        // cannot call clear here because drawing is still saving

        // calls submit function on game manager to call for drawing processing and uploading to firebase
    }

}
