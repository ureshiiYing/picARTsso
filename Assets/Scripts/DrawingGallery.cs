﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using Photon.Realtime;
using TMPro;
using System;

public class DrawingGallery : MonoBehaviour
{
    
    public UploadDownloadDrawing uploader;
    public Texture2D defaultTexture;
    public GameObject saveButton;
    public GameObject instructionsPanel;

    private string[] downloadPaths;
    private int[] randomisedDownloadPaths;
    private int drawingIndex = 0; // pointer of randomised array
    private int winnerIndex = -1;
    private bool showInstructionText = false;


    // judging screen is only loaded after everyone has submitted
    public void OnEnable()
    {
        drawingIndex = 0;
        winnerIndex = -1;

        saveButton.SetActive(true);

        DisplayInstructionsToToggle();

        // todiscuss: wait until the drawing is loaded
        // in actual game, the drawing from index 0 should be displayed
        LoadDrawing(GetActualIndexOfDownloadPathAt(drawingIndex));

        
    }

    public void OnDisable()
    {
        saveButton.SetActive(false);
    }

    public void DisplayInstructionsToToggle()
    {
        Debug.Log("display");
        if (!showInstructionText)
        {
            StartCoroutine(CoDisplayInstructions());
        }
    }

    private IEnumerator CoDisplayInstructions()
    {
        instructionsPanel.SetActive(true);
        instructionsPanel.GetComponentInChildren<TMP_Text>().text = "To toggle between the drawings, " +
            "you can tap on the left/ right sides of the screen.";
        yield return new WaitForSecondsRealtime(3f);
        instructionsPanel.SetActive(false);
        showInstructionText = true;
    }

    // called by game manager to set the paths for gallery to use
    public void SetDownloadPaths(Player[] players)
    {
        Debug.Log(players);
        downloadPaths = new string[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            downloadPaths[i] = players[i].CustomProperties["URL"].ToString();
            //Debug.Log("URL is " + downloadPaths[i]);
            Debug.Log("set download url " + i);
        }

        randomisedDownloadPaths = RandomiseIntArray(players.Length);
    }

    public string[] GetDownloadPaths()
    {
        return downloadPaths;
    }


    public int[] RandomiseIntArray(int len)
    {
        int[] result = new int[len];
        // initialise the array
        for (int i = 0; i < len; i++)
        {
            result[i] = i;
        }
        // randomise Knuth Shuffle algo
        for (int i = 1; i < len; i++)
        {
            int rnd = UnityEngine.Random.Range(0, i);
            // swap values at index i and rnd
            int temp = result[i];
            result[i] = result[rnd];
            result[rnd] = temp;
        }

        return result;
    }

    private int GetActualIndexOfDownloadPathAt(int index)
    {
        return randomisedDownloadPaths[index];
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
            drawingIndex = randomisedDownloadPaths.Length - 1;

        }
        // else display a default drawing
        else if (drawingIndex >= randomisedDownloadPaths.Length)
        {
            Debug.Log("exceeded right");
            drawingIndex = 0;
        }
        //else
        //{
        //    Debug.Log("sth wrong");
        //    uploader.SetDisplay(defaultTexture);
        //}

        LoadDrawing(GetActualIndexOfDownloadPathAt(drawingIndex));

        Debug.Log("showing: " + drawingIndex);
    }


    public int GetWinner()
    {
        winnerIndex = GetActualIndexOfDownloadPathAt(drawingIndex);
        return winnerIndex;
    }

    // to be called by save button click
    public void SaveCurrentDrawing()
    {
        try
        {
            uploader.SaveDrawingOnDevice(downloadPaths[GetActualIndexOfDownloadPathAt(drawingIndex)]);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        finally
        {
            // show a pop up when it's successful
            // check if application.persistentdatapath NGallery folder is empty, then show popup
            FindObjectOfType<ErrorMessagesHandler>().DisplayError("Successfully saved inside your gallery.");
        }
    }

}
