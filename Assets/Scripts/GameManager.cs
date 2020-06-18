using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public enum EventCodes : byte
    {
        StartNewRound,
        SetNextHost,
        RefreshTimer
    }

    //public enum GameState
    //{
    //    Starting = 0,
    //    HostPlaying = 1, // part when host inputting and player waiting is occuring
    //    PlayerPlaying = 2, // part when host waits and player is drawing
    //    Judging = 3,
    //    Ending = 4
    //}



    /* To do:
    - the transition after on submit
    - so we have a move to waiting room method too?

    */
    #region Fields
    // fields for recording game settings
    private int timeLimit;
    private int hostTimeLimit = 30;
    private int numOfPointsToWin;
    private int numOfPlayers; // forgot if this is necessary or i can get it from photonRoom?

    // fields for recording players
    private static Player[] players;
    private int currHost;

    // GameObject
    [SerializeField] private GameObject hostPanel;
    [SerializeField] private GameObject waitingRoom;

    // timer studd
    private int currTime;
    private Coroutine CoTimer;
    [SerializeField] private GameObject timerUI;

    #endregion

    #region Photon
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code >= 200) return;

        Debug.Log("event :<");
        EventCodes e = (EventCodes)photonEvent.Code;
        object[] o = (object[])photonEvent.CustomData;

        switch (e)
        {
            case EventCodes.StartNewRound:
                StartNewRound_R();
                break;

            //case EventCodes.SetNextHost:
            //    SetNextHost_R(o);
            //    break;

            case EventCodes.RefreshTimer:
                Debug.Log("refresh pls");
                RefreshTimer_R(o);
                break;
        }
    }

    #endregion

    // this space left for if using any start/ update/ on enable methods
    private void Start()
    {
        InitialiseGame();
        if (PhotonNetwork.IsMasterClient)
        {
            StartNewRound_S();
        }
        
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    #region Methods

    // initialise game settings
    public void InitialiseGame()
    {
        timeLimit = (int) PhotonNetwork.CurrentRoom.CustomProperties["TimeLimit"];
        numOfPointsToWin = (int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPoints"];
        numOfPlayers = PhotonNetwork.PlayerList.Length;
        players = PhotonNetwork.PlayerList;
        
        currHost = 0;
        // other method involving actor number
        //for (int i = 0; i < numOfPlayers; i++)
        //{
        //    if (PhotonNetwork.IsMasterClient)
        //    {
                
        //    }
        //    players[i] = PhotonNetwork.PlayerList[i];
        //}


    }

    

    // TODO: make everyone call this
    public void SetNextHost() //int hostIndex)
    {
        if (currHost == (numOfPlayers - 1)) {
            currHost = 0;
        }
        else
        { 
            currHost += 1;
        }
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

    private void InitializeTimer(int time)
    {
        Debug.Log("Initialising");
        currTime = time;
        RefreshTimerUI();

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("start coroutine");
            CoTimer = StartCoroutine(Timer());
        }
    }

    private void RefreshTimerUI()
    {
        string minutes = (currTime / 60).ToString("00");
        string seconds = (currTime % 60).ToString("00");
        timerUI.GetComponent<TMP_Text>().text = $"{minutes}:{seconds}";
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(1f);

        currTime -= 1;
        Debug.Log(currTime);

        if (currTime <= 0) 
        {
            CoTimer = null;
            // end the game...
        }
        else
        {
            RefreshTimer_S();
            CoTimer = StartCoroutine(Timer());
        }
    }


    public string[] GetArrayOfDownloadPaths()
    {
        string[] downloadPaths = new string[numOfPlayers - 1]; // exclude the host

        for(int i = 0; i < numOfPlayers; i++)
        {
            // nd to exclude host
            downloadPaths[i] = players[i].CustomProperties["URL"].ToString();
        }

        return downloadPaths;
    }



    #endregion


    #region Events

    public void StartNewRound_S()
    {
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.StartNewRound,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void StartNewRound_R()
    {
        // supp to clear all drawings, redirect normal players to waiting room and host to host room
        // redirect the players
        if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[currHost])
        {
            hostPanel.SetActive(true);
        }
        else
        {
            waitingRoom.SetActive(true);
        }

        InitializeTimer(hostTimeLimit);
    }


    public void RefreshTimer_S()
    {
        object[] package = new object[] { currTime };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.RefreshTimer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }
    
    // ask everyone else to refresh their timer
    public void RefreshTimer_R(object[] data)
    {
        Debug.Log("receive refresh");
        currTime = (int)data[0];
        RefreshTimerUI();
    }

    #endregion
}
