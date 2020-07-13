using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using Photon.Realtime;

public class DrawingGallery : MonoBehaviour
{
    
    public UploadDownloadDrawing uploader;
    public Texture2D defaultTexture;
    public GameObject saveButton;

    private string[] downloadPaths;
    private int drawingIndex = 0;
    private int winnerIndex = -1;


    // judging screen is only loaded after everyone has submitted
    public void OnEnable()
    {
        drawingIndex = 0;
        winnerIndex = -1;

        saveButton.SetActive(true);

        // in actual game, the drawing from index 0 should be displayed
        LoadDrawing(drawingIndex);
        
    }

    public void OnDisable()
    {
        saveButton.SetActive(false);
    }


    // called by game manager to set the paths for gallery to use
    public void SetDownloadPaths(Player[] players)
    {
        downloadPaths = new string[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            downloadPaths[i] = players[i].CustomProperties["URL"].ToString();
            //Debug.Log("URL is " + downloadPaths[i]);
            Debug.Log("set download url " + i);
        }

    }

    // to be called once when the judgingUI loads
    public void LoadDrawing(int index)
    {
        // download drawings onto the display
        uploader.DownloadDrawing(downloadPaths[index]);
    }
    

    // 0 to go left, 1 to go right
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
        } 
        else
        {
            Debug.Log("wrong value: " + value);
        }

        // if within player number limits
        //if (drawingIndex >= 0 && drawingIndex < downloadPaths.Length)
        //{
        //    LoadDrawing(drawingIndex);
        //}
        //else 
        if (drawingIndex < 0)
        {
            Debug.Log("preceeding");
            drawingIndex = downloadPaths.Length - 1;

        }
        // else display a default drawing
        else if (drawingIndex >= downloadPaths.Length)
        {
            Debug.Log("exceeded right");
            drawingIndex = 0;
        }
        //else
        //{
        //    Debug.Log("sth wrong");
        //    uploader.SetDisplay(defaultTexture);
        //}

        LoadDrawing(drawingIndex);

        Debug.Log("showing: " + drawingIndex);
    }


    public int GetWinner()
    {
        winnerIndex = drawingIndex;
        return winnerIndex;
    }

    // to be called by save button click
    public void SaveCurrentDrawing()
    {
        uploader.SaveDrawingOnDevice(downloadPaths[drawingIndex]);
    }

}
