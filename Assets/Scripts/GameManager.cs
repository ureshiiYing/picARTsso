using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /* To do:
    - the transition after on submit
    - so we have a move to waiting room method too?

    */
    #region Fields
    // fields for recording game settings
    private int timeLimit;
    private int numOfPointsToWin;
    private string roomName;
    private int numOfPlayers; // forgot if this is necessary or i can get it from photonRoom?

    // fields for recording players
    private static PlayerInfo[] players;
    private int myIndex; // represent this player in the players array
    private int currHost;

    #endregion

    
    // this space left for if using any start/ update/ on enable methods

    #region Methods

    // initialise game settings
    public void InitialiseGame()
    {
        timeLimit = (int) PhotonNetwork.CurrentRoom.CustomProperties["TimeLimit"];
        numOfPointsToWin = (int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPoints"];

    }

    // clear all drawings, redirect all players to waiting room
    public void StartNewRound()
    {

    }

    public void SetHost(int index)
    {

    }

    public void SetWinnerOfThisRound()
    {

    }

    // Check if winning condition is met
    private void ScoreCheck()
    {

    }

    private void EndGame()
    {

    }

    public string[] GetArrayOfDownloadPaths()
    {
        string[] downloadPaths = new string[numOfPlayers];

        for(int i = 0; i < numOfPlayers; i++)
        {
            downloadPaths[i] = players[i].DrawingURL;
        }

        return downloadPaths;
    }



    #endregion

}
