using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public enum EventCodes : byte
    {
        StartNewRound,
        SetNextHost,
        RefreshTimer,
        OnAllSubmitted
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

    // fields for recording players
    private static Player[] players;
    private int currHost;

    // GameObject
    [SerializeField] private GameObject hostPanel;
    [SerializeField] private GameObject waitingRoom;
    [SerializeField] private WordGenController wordGenerator;
    [SerializeField] private UploadDownloadDrawing uploader;
    [SerializeField] private GameObject drawingUI;
    [SerializeField] private GameObject judgingUI;
    [SerializeField] private GameObject pickPanel;

    // timer stuff
    private int currTime;
    private Coroutine CoTimer;
    [SerializeField] private GameObject timerUI;

    

    #endregion

    #region Photon
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code >= 200) return;

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
                RefreshTimer_R(o);
                break;

            case EventCodes.OnAllSubmitted:
                OnAllSubmitted_R();
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

    public void OnHostDone()
    {
        wordGenerator.OnWordConfirmation();
        // stop the host countdown timer
        StopCoroutine(CoTimer);

        InitializeTimer(timeLimit);

        
    }

    // save the upload string, update player status
    public void OnSubmit()
    {
        // upload and saves the drawing
        uploader.Save();

        // transition from drawingUI to waitingUI
        drawingUI.SetActive(false);
        waitingRoom.SetActive(true);

        // set player's bool hasSubmitted
        StartCoroutine(CoSubmission());

        //change scoreboard toggle => to be implemented after implementing scoreboard behaviour

        

    }

    private IEnumerator CoSubmission()
    {
        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable();
        playerProps.Add("hasSubmitted", true);
        Debug.Log("added");
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        yield return new WaitForSeconds(1f);

        // check if everyone has submitted
        if (DidAllSubmit())
        {
            OnAllSubmitted_S();
        }
    }

    private bool DidAllSubmit()
    {
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            // if this player is not the host
            if (player != PhotonNetwork.PlayerList[currHost])
            {
                // if this player hasnt submit
                if ((bool)player.CustomProperties["hasSubmitted"] == false)
                {
                    return false;
                }
            }
        }
        

        return true;
    }

    


    public void SetWinnerOfThisRound()
    {

    }

    // Check if winning condition is met
    private void ScoreCheck()
    {

    }

    // TODO: make everyone call this
    public void SetNextHost() //int hostIndex)
    {
        int numOfPlayers = PhotonNetwork.PlayerList.Length;
        if (currHost == (numOfPlayers - 1))
        {
            currHost = 0;
        }
        else
        {
            currHost += 1;
        }
    }

    private void EndGame()
    {

    }

    private void InitializeTimer(int time)
    {
        currTime = time;
        RefreshTimerUI();

        if (PhotonNetwork.IsMasterClient)
        {
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
        yield return new WaitForSecondsRealtime(1f);

        currTime -= 1;
        Debug.Log(currTime);

        if (currTime < 0) 
        {
            CoTimer = null;
            yield break;
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
        int numOfPlayers = PhotonNetwork.PlayerList.Length;
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
        currTime = (int)data[0];
        RefreshTimerUI();
    }

    public void OnAllSubmitted_S()
    {
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.OnAllSubmitted,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void OnAllSubmitted_R()
    {
        // load everyone into the judging scene
        waitingRoom.SetActive(false);
        judgingUI.SetActive(true);

        // only host can see pick button
        if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[currHost])
        {
            pickPanel.SetActive(true);
        }
        else
        {
            pickPanel.SetActive(false);
        }
    }


    #endregion
}
