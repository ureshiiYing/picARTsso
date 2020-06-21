using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class Scoreboard : MonoBehaviour
{
    [SerializeField]
    private Transform scoreContainer;
    [SerializeField]
    private GameObject scoreListingPrefab;

    private Player[] sortedPlayers;

    // Start is called before the first frame update
    void Start()
    {
        // initialise score for everyone
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            StartCoroutine(CoScore(player));
        }
        StartCoroutine(CoRefresh());

    }

    private IEnumerator CoScore(Player player)
    {
        ExitGames.Client.Photon.Hashtable playerOps = new ExitGames.Client.Photon.Hashtable();
        playerOps.Add("Score", 0);
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
        playerOps.Add("Score", (int)player.CustomProperties["Score"] + 1);
        player.SetCustomProperties(playerOps);
        yield return new WaitForSeconds(1f);

        // refresh the scoreboard
        StartCoroutine(CoRefresh());
    }


    private IEnumerator CoRefresh()
    {
        yield return new WaitForSeconds(1f);
        RefreshScoreboard();
    }
        

    public Player[] SortByScore()
    {
        int len = PhotonNetwork.PlayerList.Length;
        Player[] players = new Player[len];
        
        // local copy
        for(int i = 0; i < len; i++)
        {
            players[i] = PhotonNetwork.PlayerList[i];
        }
        // selection sort
        for (int i = 0; i < len; i ++)
        {
            int currScore = (int)players[i].CustomProperties["Score"];
            int currIndex = i; // bcuz have to separate out score and player
            for (int j = i; j < len; j ++)
            {
                int tempScore = (int)players[j].CustomProperties["Score"];
                if (tempScore > currScore)
                {
                    currScore = tempScore;
                    currIndex = j;
                }
            }
            //swap
            Player temp = players[i];
            players[i] = players[currIndex];
            players[currIndex] = temp;
            Debug.Log("score sort " + i + "  " + players[i].CustomProperties["Score"]);
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
        Player[] players = SortByScore();
        foreach (Player player in players)
        {
            Debug.Log(player.CustomProperties["Score"].ToString());
            GameObject tempListing = Instantiate(scoreListingPrefab, scoreContainer);
            TMP_Text scoreText = tempListing.transform.GetChild(1).GetComponent<TMP_Text>();
            TMP_Text nameText = tempListing.transform.GetChild(2).GetComponent<TMP_Text>();
            scoreText.text = player.CustomProperties["Score"].ToString();
            nameText.text = player.NickName.ToString();
        }
    }

    public void ToggleSubmissionStatus(Player player)
    {
        Toggle submissionStatus = null;

        // find the ith position of the player in the sorted player array
        for (int i = 0; i < sortedPlayers.Length; i++)
        {
            if (sortedPlayers[i] == player)
            {
                submissionStatus = scoreContainer.GetChild(i).gameObject.GetComponentInChildren<Toggle>();
            }
        }


        if (submissionStatus != null)
        {
            bool isActive = submissionStatus.isOn;
            submissionStatus.isOn = !isActive;
        }
        else
        {
            Debug.LogError("please set submission toggle");
        }
    }

}
