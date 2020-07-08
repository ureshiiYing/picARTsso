﻿using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Storage;
using Photon.Pun;
using UnityEngine.SocialPlatforms;
using System.IO;

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

    // upload and save drawing onto the cloud
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
        // start uploading
        StartCoroutine(CoUpload(screenshot));
    }

    private IEnumerator CoUpload(Texture2D screenshot)
    {
        if (screenshot != null)
        {
            // create a storage ref
            var storage = FirebaseStorage.DefaultInstance;
            string filePath;

            // if a photon room exists
            if (PhotonNetwork.CurrentRoom != null)
            {
                string roomName = PhotonNetwork.CurrentRoom.Name;
                //create a new folder for each game room-- > require a game room id instead of drawings
                filePath = $"/{roomName}/{Guid.NewGuid()}.png";
            }
            else
            {
                filePath = $"/drawings/{Guid.NewGuid()}.png";
            }
            Debug.Log(filePath);

            var screenshotRef = storage.GetReference(filePath);

            // convert texture2d into bytes
            var bytes = screenshot.EncodeToPNG();
            // add any metadata relevant
            var metadataChange = new MetadataChange()
            {
                ContentEncoding = "image/png"
            };


            var uploadTask = screenshotRef.PutBytesAsync(bytes, metadataChange);
            yield return new WaitUntil(() => uploadTask.IsCompleted);

            // handle error
            if (uploadTask.Exception != null)
            {
                Debug.LogError($"Failed to upload because {uploadTask.Exception}");
                yield break;
            }
            Debug.Log("uploaded");

            // clear all drawings
            DrawManager drawManager = FindObjectOfType<DrawManager>();
            if (drawManager != null)
            {
                drawManager.Clear();
            }
            else
            {
                Debug.Log("no draw manager");
            }

            // no error continue to get a download url ==> technically not necessary for this game.
            // can keep this part for the save function
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

            // set player URL
            if (PhotonNetwork.CurrentRoom != null)
            {
                yield return StartCoroutine(CoUpdatePlayerURL());
                Debug.Log("set");
            }
            else
            {
                Debug.Log("url not set in local player ppty");
            }
        }

    }

    private IEnumerator CoUpdatePlayerURL()
    {
        // code to set player URL
        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable();
        playerProps.Add("URL", downloadURL);
        Debug.Log("added");
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        yield return new WaitForSeconds(1f);
        
        Debug.Log(PhotonNetwork.LocalPlayer.CustomProperties["URL"].ToString());
    }

    public string GetDownloadURL()
    {
        return downloadURL;
    }


    // to be used to download and display the drawings
    // obtain an array of paths from the users submission
    public void DownloadDrawing(string path)
    {
        StartCoroutine(CoDownloadDrawing(path));
    }

    private IEnumerator CoDownloadDrawing(string path)
    {
        // if the path not null
        if (path != null)
        {
            var storage = FirebaseStorage.DefaultInstance;
            var screenshotRef = storage.GetReferenceFromUrl(path);
            Debug.Log("drawing obtained!");

            var downloadTask = screenshotRef.GetBytesAsync(4 * 4096 * 4096);  // max file memory
            yield return new WaitUntil(() => downloadTask.IsCompleted);

            // handle any error
            if (downloadTask.Exception != null)
            {
                Debug.LogError("Failed to download"); //"because " + downloadTask.Exception);
            }

            var texture = new Texture2D(2, 2);
            texture.LoadImage(downloadTask.Result);

            // display the downloaded image
            SetDisplay(texture);
            Debug.Log("texture displayed");
        } 
        else
        {
            // display a default image? skip over?
            Debug.LogError("Cannot download as no path provided");
        }
    }

    public void SetDisplay(Texture2D texture)
    {
        if (texture != null)
        {
            display.GetComponent<RawImage>().texture = texture;
        }
    }

    public void DeleteDrawing(string path)
    {
        StartCoroutine(CoDeleteDrawing(path));
    }

    private IEnumerator CoDeleteDrawing(string path)
    {
        // given an input
        if (path != null)
        {
            var storage = FirebaseStorage.DefaultInstance;
            var screenshotRef = storage.GetReferenceFromUrl(path);

            var deleteTask = screenshotRef.DeleteAsync();
            yield return new WaitUntil(() => deleteTask.IsCompleted);

            // handle error
            if (deleteTask.Exception != null)
            {
                Debug.LogError("Failed to delete"); //because " + deleteTask.Exception);
            }
            else
            {
                Debug.Log("Successfully deleted");
            }
        }
        else
        {
            Debug.LogError("Cannot delete as no download path provided");
        }
    }

    public void SaveDrawingOnDevice(string path)
    {
        StartCoroutine(CoSaveDrawingOnDevice(path));
    }

    private IEnumerator CoSaveDrawingOnDevice(string path)
    { 
        if (path != null)
        {
            var storage = FirebaseStorage.DefaultInstance;
            var screenshotRef = storage.GetReferenceFromUrl(path);
            string local_path = "";

            // Create local filesystem URL
            if (Application.platform == RuntimePlatform.Android)
            {
                local_path = Application.persistentDataPath + "/Saved Drawings";
                
            }
            else
            {
                // unity editor testing
                local_path = Application.dataPath + "/Saved Drawings";
            }

            // check if the directory exists, else create a folder which will store the pictures
            try
            {    
                if (Directory.Exists(local_path))
                {
                    Debug.Log("saved path already exists");
                }

                var newDir = Directory.CreateDirectory(local_path);
                Debug.Log("created save path dir");
            }
            catch (Exception e)
            {
                Debug.Log("failed because of " + e.ToString());
            }

            string local_url = local_path + "/" + screenshotRef.Name;
            Debug.Log(local_url);

            if (File.Exists(local_url))
            {
                // there exists a file with the same name
                // in this case, change the file name
                local_url = local_path + $"/{Guid.NewGuid()}.png";
            }

            //Download to the local filesystem
            var saveTask = screenshotRef.GetFileAsync(local_url);
            yield return new WaitUntil(() => saveTask.IsCompleted);

            // handle error
            if (saveTask.Exception != null)
            {
                Debug.LogError("Failed to save drawing"); //because " + saveTask.Exception);
                Debug.LogError(saveTask.Exception);
            }
            else
            {
                Debug.Log("Successfully saved to " + local_url);
            }

            // final check that it is saved
            if (File.Exists(local_url))
            {
                Debug.Log("the file is saved and exists");
            }
            else
            {
                Debug.Log("where's the file?");
            }


        }
        else
        {
            Debug.Log("No download path provided");
        }
    }

}
