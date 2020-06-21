using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun; // to access what theme the player selected?
using TMPro;

public class WordGenController : MonoBehaviour
{

    // main fn: generate word

    [SerializeField]
    private TextAsset textFile; // .txt file for the theme
    private string[] words;
    [SerializeField]
    private Text wordDisplay;
    [SerializeField]
    private InputField playerWord;

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

    public string GetWord()
    {
        //clear text of player input
        hostWord = playerWord.text;
        playerWord.text = "";
        string word = wordDisplay.text + " + " + hostWord;
        GenerateWord();
        return word;
    }


}
