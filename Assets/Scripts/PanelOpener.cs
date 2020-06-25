using UnityEngine;

public class PanelOpener : MonoBehaviour
{
    public GameObject panel;

    // called when the button is clicked
    // used to toggle the panel
    public void TogglePanel()
    {
        if(panel != null)
        {
            bool isActive = panel.activeSelf;
            panel.SetActive(!isActive);
        }
    }

    public void ClosePanel()
    {
        if(panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void SwitchToPanel(GameObject thatPanel)
    {
        if(panel != null && thatPanel != null)
        {
            panel.SetActive(false);
            thatPanel.SetActive(true);
        }
    }
}
