using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    // fields for other player info
    private string playerName;
    private bool isHost;
    private bool hasSubmittedDrawing;
    private int currScore;
    

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
