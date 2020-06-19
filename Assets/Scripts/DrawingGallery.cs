using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Assertions;

public class DrawingGallery : MonoBehaviour
{
    public GameManager gameManager; 

    public UploadDownloadDrawing uploader;
    public Texture2D defaultTexture;
    public GameObject winnerPanel;
    public TMP_Text winnerNameText;

    private string[] downloadPaths;
    private int drawingIndex = -1;
    private int winnerIndex = -1;
    

    // Start is called before the first frame update
    public void OnEnable()
    {
        // judging screen is only loaded after everyone has submitted

        // obtain the downloadPaths from the game manager
        downloadPaths = gameManager.GetArrayOfDownloadPaths();
        //downloadPaths = new string[1]; // num of players obtain frm gameManager

        // obtain all the download paths from the players
        // need to iterate through player info? to obtain the download paths?
        //for(int i = 0; i < 1; i++)
        //{
        //    downloadPaths[i] = uploader.GetDownloadURL();
        //    Debug.Log("URL is " + downloadPaths[i]);
        //    Debug.Log("set download url " + i);
        //}

        // default texture is set for testing
        // in actual game, the drawing from index 0 should be displayed
        uploader.SetDisplay(defaultTexture);
        
    }

    

    // just for testing purposes: 
    public void Refresh()
    {
        string URL = uploader.GetDownloadURL();
        if (URL != null)
        {
            for (int i = 0; i < 1; i++)
            {
                downloadPaths[i] = uploader.GetDownloadURL();
                Debug.Log("URL is " + downloadPaths[i]);
                Debug.Log("set download url " + i);
            }
        } 
        else
        {
            Debug.Log("Not Ready Yet");
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
        if (drawingIndex >= 0 && drawingIndex < 1) // instead of 1 should be player numbers
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


    public void SetWinner()
    {
        StartCoroutine(CoSetWinner());

        // maybe add animations next time if i wanna be fancy
    }

    private IEnumerator CoSetWinner()
    {
        winnerIndex = drawingIndex;

        // display the winning drawing
        LoadDrawing(winnerIndex);

        winnerNameText.GetComponent<TMP_Text>().text = "Player " + winnerIndex + " !"; // set to player name
        // pop up something to show that this is the winner for this round
        winnerPanel.SetActive(true);

        yield return new WaitForSeconds(2f);

        winnerPanel.SetActive(false);

        // should trigger the next round... so this code should be in game manager script
    }


}
