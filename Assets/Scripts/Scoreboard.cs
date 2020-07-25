using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using System;

public class Scoreboard : MonoBehaviour
{
    [SerializeField]
    private Transform scoreContainer;
    [SerializeField]
    private GameObject scoreListingPrefab;

    private Player[] sortedPlayers;
    private Texture2D[] avatarsFile;

    // Start is called before the first frame update
    void Start()
    {
        // initialise score for everyone
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            StartCoroutine(CoScore(player));
        }
        StartCoroutine(CoRefresh());

        avatarsFile = Resources.LoadAll<Texture2D>("Avatar");
    }

    private IEnumerator CoScore(Player player)
    {
        ExitGames.Client.Photon.Hashtable playerOps = new ExitGames.Client.Photon.Hashtable();
        playerOps.Add("Score", 0);
        playerOps.Add("ReportCount", 0);
        playerOps.Add("hasSubmitted", false);
        playerOps.Add("IsKicked", false);
        player.SetCustomProperties(playerOps);
        yield return new WaitForSeconds(1f);
    }

    //public void IncrementScore(Player player)
    //{
    //    StartCoroutine(CoIncrementScore(player));
    //}

    public IEnumerator CoIncrementScore(Player player)
    {
        ExitGames.Client.Photon.Hashtable playerOps = new ExitGames.Client.Photon.Hashtable();
        int newScore = (int)player.CustomProperties["Score"] + 1;
        Debug.Log("updating score: " + newScore);
        playerOps.Add("Score", newScore);
        player.SetCustomProperties(playerOps);
        yield return new WaitForSeconds(1f);

        Debug.Log("new score: " + (int)player.CustomProperties["Score"]);
    }


    public IEnumerator CoRefresh()
    {
        yield return new WaitForSeconds(1f);
        RefreshScoreboard();
    }


    public Player[] SortByScore(Player[] playersToSort)
    {
        int len = playersToSort.Length;
        Player[] players = new Player[len];

        // local copy
        for (int i = 0; i < len; i++)
        {
            players[i] = playersToSort[i];
        }
        // selection sort
        for (int i = 0; i < len; i++)
        {
            int iScore = (int)players[i].CustomProperties["Score"];
            int currIndex = i; // bcuz have to separate out score and player
            bool iKicked = (bool)players[i].CustomProperties["IsKicked"];
            for (int j = i; j < len; j++)
            {
                int jScore = (int)players[j].CustomProperties["Score"];
                bool jKicked = (bool)players[i].CustomProperties["IsKicked"];

                if (jScore > iScore && (iKicked == jKicked) || iKicked)
                {
                    iScore = jScore;
                    currIndex = j;
                }
            }
            //swap
            Player temp = players[i];
            players[i] = players[currIndex];
            players[currIndex] = temp;

            Debug.Log(players[i].NickName + ": " + (int)players[i].CustomProperties["Score"]);
        }

        sortedPlayers = players;
        return players;
    }

    public void RefreshScoreboard()
    {
        ClearScore();
        ListScore();
    }

    public void ClearScore()
    {
        for (int i = scoreContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(scoreContainer.GetChild(i).gameObject);
        }
    }

    public void ListScore()
    {
        Player[] players = SortByScore(PhotonNetwork.PlayerList);
        foreach (Player player in players)
        {
            if (!(bool)player.CustomProperties["IsKicked"])
            {
                GameObject tempListing = Instantiate(scoreListingPrefab, scoreContainer);
                TMP_Text scoreText = tempListing.transform.GetChild(1).GetComponent<TMP_Text>();
                TMP_Text nameText = tempListing.transform.GetChild(2).GetComponent<TMP_Text>();
                GameObject reportButton = tempListing.transform.GetChild(0).gameObject;
                RawImage avatar = tempListing.transform.GetChild(3).GetComponent<RawImage>();
                Toggle toggle = tempListing.transform.GetChild(4).GetComponent<Toggle>();

                scoreText.text = player.CustomProperties["Score"].ToString();
                nameText.text = player.NickName.ToString();
                int ptr = (int) player.CustomProperties["Avatar"];
                avatar.texture = avatarsFile[ptr];
                toggle.isOn =  (bool)player.CustomProperties["hasSubmitted"];
                Debug.Log(player.NickName + " scoreboard" + player.CustomProperties["hasSubmitted"].ToString() + 
                    " toggle: " + toggle.isOn );

                if (player != PhotonNetwork.LocalPlayer)
                {
                    reportButton.GetComponent<ReportButton>().WhenInstantiated(player);
                }
                else
                {
                    reportButton.SetActive(false);
                }
            }
        }
    }
    
    public void ToggleSubmissionStatus(Player player, bool hasSubmitted)
    {
        Toggle submissionStatus = null;

        // find the ith position of the player in the sorted player array
        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            if (sortedPlayers[i] == player)
            {
                try
                {
                    Debug.Log(sortedPlayers[i].NickName);
                    submissionStatus = scoreContainer.GetChild(i).gameObject.GetComponentInChildren<Toggle>();
                } 
                catch (Exception e)
                {
                    Debug.Log(sortedPlayers[i].NickName);
                    Debug.Log(scoreContainer.GetChild(i) + " " + e);
                }
            }
        }


        if (submissionStatus != null)
        {
            submissionStatus.isOn = hasSubmitted;
            Debug.Log(player.NickName + " toggle:" + submissionStatus.isOn + " ppty:" + hasSubmitted);
        }
        else
        {
            Debug.LogError("please set submission toggle");
        }
    }

    public object[] GetTopThreePlayers()
    {
        object[] res = new object[6];

        for (int i = 0; i < sortedPlayers.Length; i++)
        {
            if (i > 3)
            {
                break;
            }

            res[i] = sortedPlayers[i].NickName;
            res[i + 3] = avatarsFile[(int)sortedPlayers[i].CustomProperties["Avatar"]];
        }

        
        return res;
    }



}
