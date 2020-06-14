using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DrawingGallery : MonoBehaviour
{
    // public GameObject gameManager; 
    // ^ use uploader object temporarily
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
        // load a waiting screen? need to wait for everyone's download URL 
        downloadPaths = new string[1]; // num of players obtain frm gameManager

        // obtain all the download paths from the players
        // need to iterate through player info? to obtain the download paths?
        //for(int i = 0; i < 1; i++)
        //{
        //    downloadPaths[i] = uploader.GetDownloadURL();
        //    Debug.Log("URL is " + downloadPaths[i]);
        //    Debug.Log("set download url " + i);
        //}

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

    public void NextDrawing()
    {

        // if within player number limits
        if ((drawingIndex + 1) >= 0 && drawingIndex < 2)
        {
            drawingIndex += 1;
            LoadDrawing(drawingIndex);
        }
        // else do not change drawing
        else
        {
            drawingIndex += 1;
            uploader.SetDisplay(defaultTexture);
        }

        Debug.Log("showing: " + drawingIndex);
    }

    public void PreviousDrawing()
    {

        // if within player number limits
        if ((drawingIndex - 1) >= 0 && drawingIndex < 1)
        {
            drawingIndex -= 1;
            LoadDrawing(drawingIndex);
        }
        // else do not change drawing
        else
        {
            drawingIndex -= 1;
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
        winnerNameText.GetComponent<TMP_Text>().text = "Player " + winnerIndex + " !"; // set to player name
        // pop up something to show that this is the winner for this round
        winnerPanel.SetActive(true);

        yield return new WaitForSeconds(2f);

        winnerPanel.SetActive(false);

        // should trigger the next round... so this code should be in game manager script
    }
}
