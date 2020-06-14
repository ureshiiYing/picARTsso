using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    // fields for other player info


    // field for drawingURL
    private string drawingURL;
    public string DrawingURL {
        get
        {
            return drawingURL;
        }
        set
        {
            drawingURL = value; 
        }
    }



}
