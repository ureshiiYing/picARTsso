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
        Debug.Log("1st" + PhotonNetwork.LocalPlayer.CustomProperties["Score"]);
        ExitGames.Client.Photon.Hashtable playerOps = new ExitGames.Client.Photon.Hashtable();
        playerOps.Add("Score", 0);
        player.SetCustomProperties(playerOps);
        yield return new WaitForSeconds(1f);
        Debug.Log("2nd" + PhotonNetwork.LocalPlayer.CustomProperties["Score"]);
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
        int currScore = 0;
        int currIndex = 0;
        // local copy
        for(int i = 0; i < len; i++)
        {
            players[i] = PhotonNetwork.PlayerList[i];
        }
        // selection sort
        for (int i = 0; i < len; i ++)
        {
            for (int j = i; j < len; j ++)
            {
                Debug.Log("sort" + PhotonNetwork.LocalPlayer.CustomProperties["Score"]);
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
        }
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
        Debug.Log("list" + PhotonNetwork.LocalPlayer.CustomProperties["Score"]);
        Player[] players = SortByScore();
        foreach (Player player in players)
        {
            GameObject tempListing = Instantiate(scoreListingPrefab, scoreContainer);
            TMP_Text scoreText = tempListing.transform.GetChild(1).GetComponent<TMP_Text>();
            TMP_Text nameText = tempListing.transform.GetChild(2).GetComponent<TMP_Text>();
            scoreText.text = player.CustomProperties["Score"].ToString();
            nameText.text = player.NickName.ToString();
        }
    }
}
