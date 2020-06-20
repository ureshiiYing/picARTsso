using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Assertions;
using Photon.Realtime;

public class DrawingGallery : MonoBehaviour
{
    
    public UploadDownloadDrawing uploader;
    public Texture2D defaultTexture;

    private string[] downloadPaths;
    private int drawingIndex = -1;
    private int winnerIndex = -1;
    

    // Start is called before the first frame update
    public void OnEnable()
    {
        // judging screen is only loaded after everyone has submitted

        // obtain the downloadPaths from the game manager
        

        // default texture is set for testing
        // in actual game, the drawing from index 0 should be displayed
        uploader.SetDisplay(defaultTexture);
        
    }

    
    // called by game manager to set the paths for gallery to use
    public void SetDownloadPaths(Player[] players)
    {
        downloadPaths = new string[players.Length];
        for (int i = 0; i < 1; i++)
        {
            downloadPaths[i] = players[i].CustomProperties["URL"].ToString();
            //    Debug.Log("URL is " + downloadPaths[i]);
            Debug.Log("set download url " + i);
        }

    }

    // to be called once when the judgingUI loads (on submit)? may not work cuz it's inactive...
    public void LoadDrawing(int index)
    {
        uploader.DownloadDrawing(downloadPaths[index]);
    }

    // can combine these two into one function
    public void ToggleDrawing(int value)
    {
        Assert.IsTrue(value == 0 || value == 1);
        if (value == 0)
        {
            drawingIndex -= 1;
        }
        else if (value == 1)
        {
            drawingIndex += 1;
        } else
        {
            Debug.Log("wrong value: " + value);
        }

        // if within player number limits
        if (drawingIndex >= 0 && drawingIndex < (Photon.Pun.PhotonNetwork.PlayerList.Length - 1))
        {
            LoadDrawing(drawingIndex);
        }
        // else display a default drawing
        else
        {
            uploader.SetDisplay(defaultTexture);
        }

        Debug.Log("showing: " + drawingIndex);
    }


    public int GetWinner()
    {
        winnerIndex = drawingIndex;
        return winnerIndex;
    }



}
