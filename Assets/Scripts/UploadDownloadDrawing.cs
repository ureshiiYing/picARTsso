﻿using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Storage;

public class UploadDownloadDrawing : MonoBehaviour
{
    public RawImage display;

    private string downloadURL;

    public Texture2D SaveDrawingAsTexture()
    {
        RenderTexture currTexture = RenderTexture.active;
        RenderTexture renderTexture = this.GetComponent<Camera>().targetTexture;
        RenderTexture.active = renderTexture;

        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        RenderTexture.active = currTexture;

        return texture;
    }

    public void Save()
    {
        StartCoroutine(CoSave());
    }

    private IEnumerator CoSave()
    {
        yield return new WaitForEndOfFrame();

        Texture2D drawingTex = SaveDrawingAsTexture();

        // start uploading into cloud storage
        StartUpload(drawingTex);

        // destroy the temp texture
        Destroy(drawingTex);
    }

    // used to upload the texture 
    public void StartUpload(Texture2D screenshot)
    {
        StartCoroutine(CoUpload(screenshot));
    }

    private IEnumerator CoUpload(Texture2D screenshot)
    {
        // create a storage ref
        var storage = FirebaseStorage.DefaultInstance;
        string filePath = $"/drawings/{Guid.NewGuid()}.png";
        Debug.Log(filePath);
        var screenshotRef = storage.GetReference(filePath);   
        // TODO: perhaps can create a new folder for each game room --> require a game room id instead of drawings

        // convert texture2d into bytes
        var bytes = screenshot.EncodeToPNG();
        // add any metadata relevant
        var metadataChange = new MetadataChange()
        {
            ContentEncoding = "image/png"
        };


        var uploadTask = screenshotRef.PutBytesAsync(bytes, metadataChange);
        yield return new WaitUntil(() => uploadTask.IsCompleted);
        Debug.Log("uploaded");

        // handle error
        if (uploadTask.Exception != null)
        {
            Debug.LogError($"Failed to upload because {uploadTask.Exception}");
            yield break;
        }


        // no error continue to get a download url ==> technically not necessary for this game.
        var getURLTask = screenshotRef.GetDownloadUrlAsync();
        yield return new WaitUntil(() => getURLTask.IsCompleted);

        // handle error
        if (getURLTask.Exception != null)
        {
            Debug.LogError($"Failed to get a download url because {getURLTask.Exception}");
            yield break;
        }

        Debug.Log("Download from " + getURLTask.Result);
        // ^ not necessary

        // save the storage reference of this drawing
        downloadURL = "gs://picartsso.appspot.com" + filePath;
        Debug.Log("file location is " + downloadURL);


    }

    public string GetDownloadURL()
    {
        return downloadURL;
    }

    // to be used to download and display the drawings
    // obtain an array of paths from the users submission
    public void DownloadDrawing(string path)
    {
        StartCoroutine(CoDownloadScreenshot(path));
    }

    private IEnumerator CoDownloadScreenshot(string path)
    {
        // if the path exists
        if (path != null)
        {
            var storage = FirebaseStorage.DefaultInstance;
            var screenshotRef = storage.GetReferenceFromUrl(path);
            Debug.Log("obtained!");

            var downloadTask = screenshotRef.GetBytesAsync(4 * 4096 * 4096); // file memory
            yield return new WaitUntil(() => downloadTask.IsCompleted);

            var texture = new Texture2D(2, 2);
            texture.LoadImage(downloadTask.Result);

            // display the downloaded image
            SetDisplay(texture);
            Debug.Log("texture displayed");
        } 
        else
        {
            // display a default image? skip over?
            Debug.LogError("Cannot download as no path exists");
        }
    }

    public void SetDisplay(Texture2D texture)
    {
        display.GetComponent<RawImage>().texture = texture;
    }

}
