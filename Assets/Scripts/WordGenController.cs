﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun; // to access what theme the player selected?

public class WordGenController : MonoBehaviour
{
    // if reloading scene after every round may have to donotdestroy on load and
    // make text worddisplay a prefab

    // think of how to change themes
    [SerializeField]
    private TextAsset textFile; // .txt file for the theme
    private string[] words;
    [SerializeField]
    private Text wordDisplay;
    [SerializeField]
    private InputField playerWord;

    [SerializeField]
    private GameObject playerPanel;
    [SerializeField]
    private GameObject waitingPanel;
    [SerializeField]
    private GameObject hostPanel;

    [SerializeField]
    private Text drawingWordsDisplay;
    private string hostWord;

    // Start is called before the first frame update
    void Start()
    {
        textFile = (TextAsset)Resources.Load((string) PhotonNetwork.CurrentRoom.CustomProperties["Theme"]);
        if (textFile != null)
        {
            words = (textFile.text.Split('\n')); // make array of words
            string wordForTheRound = words[Random.Range(0, words.Length)];
            wordDisplay.text = wordForTheRound;
        }
    }

    public void GenerateWord()
    {
        string wordForTheRound = words[Random.Range(0, words.Length)];
        wordDisplay.text = wordForTheRound;
    }

    public void OnWordConfirmation() // send to everyone, set word in drawing canvas
    {
        //clear text of player input
        hostWord = playerWord.text;
        playerWord.text = "";
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("StartDrawing", RpcTarget.Others, wordDisplay.text + " + " + hostWord);
        // host goes to waiting
        hostPanel.SetActive(false);
        waitingPanel.SetActive(true);
    }

    [PunRPC]
    public void StartDrawing(string wordToDisplay)
    {
        waitingPanel.SetActive(false);
        playerPanel.SetActive(true);
        drawingWordsDisplay.text += wordToDisplay;
        //start and sync timer
    }
}
