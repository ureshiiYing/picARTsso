﻿using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    #region nested classes

    public enum EventCodes : byte
    {
        StartNewRound,
        TriggerNextRound,
        SetNextHost,
        RefreshTimer,
        OnWordConfirmation,
        OnSubmitToggle,
        OnAllSubmitted,
        SetWinner,
        EndGame
    }

    public enum GameState
    {
        Starting = 0,
        HostPlaying = 1, // part when host inputting and player waiting is occuring
        PlayerPlaying = 2, // part when host waits and player is drawing
        Judging = 3,
        Ending = 4
    }

    #endregion


    #region Fields
    // fields for recording game settings
    private int timeLimit;
    private int hostTimeLimit = 30;
    private int numOfPointsToWin;

    private GameState state;

    private Player currHost;

    // GameObject
    [SerializeField] private GameObject hostPanel;
    [SerializeField] private GameObject waitingRoom;
    [SerializeField] private WordGenController wordGenerator;
    [SerializeField] private UploadDownloadDrawing uploader;
    [SerializeField] private GameObject drawingUI;
    [SerializeField] private GameObject confirmSubmitPanel;
    [SerializeField] private GameObject judgingUI;
    [SerializeField] private GameObject pickPanel;
    [SerializeField] private Scoreboard scoreboard;
    [SerializeField] private GameObject endingUI;

    // drawing stuff
    [SerializeField] private TMP_Text drawingWordsDisplay;

    // timer stuff
    private int currTime;
    private Coroutine CoTimer;
    [SerializeField] private GameObject timerUI;

    // winner for the round UI
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private TMP_Text winnerNameText;

    // game winner UI
    [SerializeField] private GameObject firstPlace;
    [SerializeField] private GameObject secPlace;
    [SerializeField] private GameObject thirdPlace;
    [SerializeField] private GameObject winnerCardPrefab;



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

            case EventCodes.TriggerNextRound:
                TriggerNextRound_R();
                break;

            case EventCodes.SetNextHost:
                SetNextHost_R(o);
                break;

            case EventCodes.RefreshTimer:
                RefreshTimer_R(o);
                break;

            case EventCodes.OnWordConfirmation:
                OnWordConfirmation_R(o);
                break;

            case EventCodes.OnSubmitToggle:
                OnSubmitToggle_R(o);
                break;

            case EventCodes.OnAllSubmitted:
                OnAllSubmitted_R();
                break;

            case EventCodes.SetWinner:
                SetWinnerOfThisRound_R(o);
                break;

            case EventCodes.EndGame:
                EndGame_R();
                break;
        }
    }

    #endregion

    // this space left for if using any start/ update/ on enable methods
    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
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
        
        currHost = PhotonNetwork.MasterClient;

        state = GameState.Starting;
    }


    // called by skip button
    // called when auto skip
    // fix this logic: called by everyone lmao?
    public void OnHostSkip()
    {
        wordGenerator.GetWord();
        TriggerNextRound_S();
    }

    public void OnSubmit()
    {
        confirmSubmitPanel.SetActive(true);
    }

    // save the upload string, update player status
    // called by the inner submit button on drawing UI
    public void OnReadyToSubmit()
    {
        // close the cfm panel if active
        confirmSubmitPanel.SetActive(false);

        // upload and saves the drawing
        uploader.Save();

        // transition from drawingUI to waitingUI
        drawingUI.SetActive(false);
        waitingRoom.SetActive(true);

        // set player's bool hasSubmitted
        StartCoroutine(CoSubmission());

        //sync the scoreboard
        OnSubmitToggle_S(PhotonNetwork.LocalPlayer);
        

    }

    private IEnumerator CoSubmission()
    {
        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable();
        playerProps.Add("hasSubmitted", true);
        Debug.Log("hasSubmitted bool added");
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        yield return new WaitForSeconds(1f);
        Debug.Log("setting submitted" + PhotonNetwork.LocalPlayer.CustomProperties["hasSubmitted"].ToString());

        Debug.Log("didallsubmit: " + DidAllSubmit());
        // check if everyone has submitted
        if (DidAllSubmit())
        {
            yield return new WaitForSeconds(1f);
            OnAllSubmitted_S();
        }
    }

    

    private bool DidAllSubmit()
    {
 
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            // if this player is not the host
            if (!player.Equals(currHost) && !(bool)player.CustomProperties["IsKicked"])
            {
                Debug.Log(player.NickName + " hasSubmitted: " + (bool)player.CustomProperties["hasSubmitted"]);
                // if this player hasnt submit
                if ((bool)player.CustomProperties["hasSubmitted"] == false)
                {
                    return false;
                }
            }
        }
        

        return true;
    }




    // Check if winning condition is met after each round
    private void ScoreCheck(Player player)
    {
        int score = (int)player.CustomProperties["Score"];
        if (score >= numOfPointsToWin)
        {
            EndGame_S();
        } 
        else
        {
            // should trigger the next round... 
            TriggerNextRound_S();
        }
    }

    // load menu scene and exit room
    public void ReturnToMainMenu()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);

    }


    //// load menu scene and into room panel
    //public void PlayAgain()
    //{
    //    SceneManager.LoadScene(0);
    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        PhotonNetwork.CurrentRoom.IsOpen = true;
    //    }
    //}

    // when current masterclient disconnects, new masterclient handles the countdown
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        if (PhotonNetwork.IsMasterClient)
        {
            InitializeTimer(currTime);
        }
    }


    // used to initialise and reinitialise timer
    private void InitializeTimer(int time)
    {
        currTime = time;
        RefreshTimerUI();

        if (PhotonNetwork.IsMasterClient)
        {
            if (CoTimer != null)
            {
                StopCoroutine(CoTimer);
                CoTimer = null;
            }
            CoTimer = StartCoroutine(Timer());
        }
    }

    private void RefreshTimerUI()
    {
        string minutes = (currTime / 60).ToString("00");
        string seconds = (currTime % 60).ToString("00");
        timerUI.GetComponent<TMP_Text>().text = $"{minutes} : {seconds}";
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSecondsRealtime(1f);

        currTime -= 1;

        if (currTime < 0 || !PhotonNetwork.IsMasterClient) 
        {
            CoTimer = null;
            
            // stop the timer
            yield break;

        }
        else
        {
            RefreshTimer_S();
            CoTimer = StartCoroutine(Timer());
            
        }
    }


    public Player[] GetArrayOfPlayersWithoutHost()
    {
        Debug.Log("getting downloads");
        int numOfPlayers = PhotonNetwork.PlayerList.Length;
        ArrayList downloadPlayer = new ArrayList(); // exclude the host
        int index = 0;

        for(int i = 0; i < numOfPlayers; i++)
        {
            // nd to exclude host and kicked ppl
            if (!PhotonNetwork.PlayerList[i].Equals(currHost) && !(bool)PhotonNetwork.PlayerList[i].CustomProperties["IsKicked"])
            {
                downloadPlayer.Add(PhotonNetwork.PlayerList[i]);
                index++;
            }
        }
        // copy to array
        Player[] res = new Player[index];
        for (int j = 0; j < index; j ++)
        {
            res[j] = (Player) downloadPlayer[j];
            Debug.Log(res[j].NickName + " + URL: " + res[j].CustomProperties["URL"].ToString());
        }

        Debug.Log(res);
        return res;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.CurrentRoom.Players.ContainsValue(otherPlayer))
        {
            if (otherPlayer.Equals(currHost))
            {
                if (state != GameState.Ending)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        TriggerNextRound_S();
                    }
                    FindObjectOfType<ErrorMessagesHandler>().DisplayError("Host has left the room... Starting new round.");
                }
            }
            else
            {
                if (state == GameState.PlayerPlaying)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        if (DidAllSubmit())
                        {
                            OnAllSubmitted_S();
                        }
                    }

                }
                else if (state == GameState.Judging)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        OnAllSubmitted_S();
                    }
                    FindObjectOfType<ErrorMessagesHandler>().DisplayError("Refreshing gallery...");
                }
            }
        }
        else if (otherPlayer.IsInactive)
        {
            if ((bool)otherPlayer.CustomProperties["IsKicked"])
            {
                if (otherPlayer.Equals(currHost))
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        TriggerNextRound_S();
                    }
                    FindObjectOfType<ErrorMessagesHandler>().DisplayError("Host has been kicked... Starting new round.");
                }
                else
                {
                    if (state == GameState.PlayerPlaying)
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            Debug.Log(DidAllSubmit())
;                            if (DidAllSubmit())
                            {
                                OnAllSubmitted_S();
                            }
                        }

                    }
                    else if (state == GameState.Judging)
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            OnAllSubmitted_S();
                        }
                        FindObjectOfType<ErrorMessagesHandler>().DisplayError("Refreshing gallery...");
                    }
                }
                
            }
        }
            
    }

    private IEnumerator CoResetSubmissionStatus()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable();
            playerProps.Add("hasSubmitted", false);
            player.SetCustomProperties(playerProps);
        }
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator CoResetRefresh()
    {
        yield return StartCoroutine(CoResetSubmissionStatus());
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.StartNewRound,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache },
            new SendOptions { Reliability = true }
        );
    }

    #endregion


    #region Events

    public void StartNewRound_S()
    {
        // reset the submission status
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(CoResetRefresh());
        }


    }

    public void StartNewRound_R()
    {
        // change back to original settings
        judgingUI.SetActive(false);
        drawingUI.SetActive(false);
        // drawingUI.GetComponent<BrushToolUI>().Clear();
        pickPanel.SetActive(false);
        confirmSubmitPanel.SetActive(false);
        timerUI.SetActive(true);
        state = GameState.HostPlaying;

        StartCoroutine(scoreboard.CoRefresh());
        // delete the drawings from previous round
        /*
        if (PhotonNetwork.IsMasterClient) {
            foreach (string path in judgingUI.GetComponent<DrawingGallery>().GetDownloadPaths())
            {
                if (path != null)
                {
                    try
                    {
                        uploader.DeleteDrawing(path);
                    }
                    catch
                    {
                        Debug.Log("could not delete");
                    }
                }
            }
        */

        // redirect normal players to waiting room and host to host room
        // redirect the players
        if (PhotonNetwork.LocalPlayer.Equals(currHost))
        {
            hostPanel.SetActive(true);
            waitingRoom.SetActive(false);   
        }
        else
        {
            hostPanel.SetActive(false);
            waitingRoom.SetActive(true);
        }

        InitializeTimer(hostTimeLimit);
    }

    public void TriggerNextRound_S()
    {
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.TriggerNextRound,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }

    // called when the round ends
    public void TriggerNextRound_R()
    {
        // change host first
        SetNextHost_S();


        Debug.Log("starting new round");


        StartNewRound_S();
    }

    public void RefreshTimer_S()
    {
        object[] package = new object[] { currTime };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.RefreshTimer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache },
            new SendOptions { Reliability = true }
        );
    }
    
    // ask everyone else to refresh their timer
    public void RefreshTimer_R(object[] data)
    {
        currTime = (int)data[0];
        RefreshTimerUI();

        if (currTime <= 0)
        {
            if (state == GameState.HostPlaying)
            {
                // if the host did not manage to input in time, automatically skipped.
                if (PhotonNetwork.IsMasterClient)
                {
                    OnHostSkip();
                }
                else if (PhotonNetwork.LocalPlayer.Equals(currHost))
                {
                    wordGenerator.GetWord();
                }
            }
            else if (state == GameState.PlayerPlaying)
            {
                // player ran out of time to draw, force submit
                if (!(bool)PhotonNetwork.LocalPlayer.CustomProperties["hasSubmitted"] 
                    && !PhotonNetwork.LocalPlayer.Equals(currHost))
                {
                    // if hasnt submit and not the host
                    OnReadyToSubmit();
                }
            }
        }
    }

    public void OnWordConfirmation_S()
    {
        string wordsToDisplay = wordGenerator.GetWord();

        object[] package = new object[] { wordsToDisplay };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.OnWordConfirmation,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache },
            new SendOptions { Reliability = true }
        );
    }

    public void OnWordConfirmation_R(object[] data)
    {
         // stop the host countdown timer
         InitializeTimer(timeLimit);
       

        string wordToDisplay = (string)data[0];

        // transition drawing players to drawingUI
        if (!PhotonNetwork.LocalPlayer.Equals(currHost)
            && !(bool)PhotonNetwork.LocalPlayer.CustomProperties["hasSubmitted"])
        {
            waitingRoom.SetActive(false);
            drawingUI.SetActive(true);

            drawingWordsDisplay.GetComponent<TMP_Text>().text = "Topics: " + wordToDisplay;
        }
        else 
        {
            // transition host to waitingRoom
            hostPanel.SetActive(false);
            waitingRoom.SetActive(true);
        }

        state = GameState.PlayerPlaying;
    }

    public void OnSubmitToggle_S(Player player)
    {
        object[] package = new object[] { player };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.OnSubmitToggle,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache },
            new SendOptions { Reliability = true }
        );
    }

    public void OnSubmitToggle_R(object[] data)
    {
        Player player = (Player)data[0];
        //change scoreboard toggle
        //scoreboard.ToggleSubmissionStatus(player, (bool)player.CustomProperties["hasSubmitted"]);
        StartCoroutine(scoreboard.CoRefresh());
        Debug.Log(player.NickName + " submitted" + player.CustomProperties["hasSubmitted"].ToString());
    }

    public void OnAllSubmitted_S()
    {
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.OnAllSubmitted,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache },
            new SendOptions { Reliability = true }
        );
    }

    public void OnAllSubmitted_R()
    {
        // stop the countdown
        if (CoTimer != null)
        {
            StopCoroutine(CoTimer);
        }
        timerUI.SetActive(false);

        judgingUI.GetComponent<DrawingGallery>().SetDownloadPaths(GetArrayOfPlayersWithoutHost());

        // load everyone into the judging scene
        waitingRoom.SetActive(false);
        judgingUI.SetActive(true);
        state = GameState.Judging;

        // only host can see pick button
        if (PhotonNetwork.LocalPlayer.Equals(currHost))
        {
            pickPanel.SetActive(true);
        }
        else
        {
            pickPanel.SetActive(false);
        }
    }

    // call from pick this button
    public void SetWinnerOfThisRound_S()
    {
        pickPanel.SetActive(false);

        int winnerIndex = judgingUI.GetComponent<DrawingGallery>().GetWinner();

        Player[] drawingPlayers = GetArrayOfPlayersWithoutHost();
        Player winner = drawingPlayers[winnerIndex];

        object[] package = new object[] { winnerIndex, winner };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.SetWinner,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache },
            new SendOptions { Reliability = true }
        );
    }

    public void SetWinnerOfThisRound_R(object[] data)
    {
        int winnerIndex = (int)data[0];
        Player winner = (Player)data[1];
        StartCoroutine(CoSetWinner(winnerIndex, winner));
    }

    private IEnumerator CoSetWinner(int winnerIndex, Player winner)
    {
        // display the winning drawing
        judgingUI.GetComponent<DrawingGallery>().LoadDrawing(winnerIndex);

        winnerNameText.GetComponent<TMP_Text>().text = winner.NickName + " !"; // set to player name
        
        // pop up something to show that this is the winner for this round
        winnerPanel.SetActive(true);

        yield return new WaitForSeconds(2f);

        winnerPanel.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("setting score");
            // set score
            yield return StartCoroutine(scoreboard.CoIncrementScore(winner));
            Debug.Log("set score done");
        }

        yield return StartCoroutine(scoreboard.CoRefresh());

        if (PhotonNetwork.IsMasterClient) 
        { 
            // when thrs only two or less players just end the game
            // this game logic abit weird
            if ((GetArrayOfPlayersWithoutHost().Length + 1) < 3)
            {
                EndGame_S();
            }
            else
            {
                // check if there is a winner otherwise
                ScoreCheck(winner);
            }
            
        }

    }

    public void SetNextHost_S()
    {
        int numOfPlayers = PhotonNetwork.PlayerList.Length;
        do
        {
            currHost = currHost.GetNext();

        } while ((bool)currHost.CustomProperties["IsKicked"]);
        
        Debug.Log(currHost + " " + currHost.NickName);

        object[] package = new object[] { currHost };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.SetNextHost,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others, CachingOption = EventCaching.AddToRoomCache },
            new SendOptions { Reliability = true }
        );
    }

    public void SetNextHost_R(object[] data) 
    {
        currHost = (Player)data[0];
    }

    public void EndGame_S()
    {
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.EndGame,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache },
            new SendOptions { Reliability = true }
        );
    }

    private void EndGame_R()
    {
        state = GameState.Ending;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.PlayerTtl = 0; // 0sec
            PhotonNetwork.CurrentRoom.EmptyRoomTtl = 0; // 0sec
        }

        GameObject[] forMyTransforms = new GameObject[3] { firstPlace, secPlace, thirdPlace };
        object[] data = scoreboard.GetTopThreePlayers();

        // display winner
        for (int i = 0; i < 3; i++)
        {
            if (data[i] != null)
            {
                SettingWinners(forMyTransforms[i], data[i].ToString(), (Texture2D)data[i + 3]);
            }
        }

        judgingUI.SetActive(false);
        endingUI.SetActive(true);
    }

    private void SettingWinners(GameObject winner, string name, Texture2D avatar)
    {
        GameObject temp = Instantiate(winnerCardPrefab, winner.transform);
        temp.GetComponentInChildren<TMP_Text>().text = name;
        temp.GetComponentInChildren<RawImage>().texture = avatar;
    }
    #endregion
}
