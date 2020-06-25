using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    #region nested classes

    public enum EventCodes : byte
    {
        StartNewRound,
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
    [SerializeField] private Scoreboard scoreboard;
    [SerializeField] private GameObject endingUI;

    // drawing stuff
    [SerializeField] private TMP_Text drawingWordsDisplay;

    // timer stuff
    private int currTime;
    private Coroutine CoTimer;
    [SerializeField] private GameObject timerUI;

    // winner UI
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private TMP_Text winnerNameText;

    // game winner UI
    [SerializeField] private TMP_Text gameWinnerText;

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

            case EventCodes.SetNextHost:
                SetNextHost_R();
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
                EndGame_R(o);
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

        state = GameState.Starting;
    }


    // called by skip button
    // called when auto skip
    public void OnHostSkip()
    {
        wordGenerator.GenerateWord();
        TriggerNextRound();
    }

    // called when the round ends
    public void TriggerNextRound()
    {
        // only host can call this
        if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[currHost])
        {
            // change host first
            SetNextHost_S();


            Debug.Log("starting new round");


            StartNewRound_S();
        }
    }

    // save the upload string, update player status
    // called by the submit button on drawing UI
    public void OnSubmit()
    {
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
        Debug.Log("added");
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        yield return new WaitForSeconds(1f);

        // check if everyone has submitted
        if (DidAllSubmit())
        {
            yield return new WaitForSeconds(1f);
            OnAllSubmitted_S();
        }
    }

    private IEnumerator CoResetSubmissionStatus()
    {
        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable();
        playerProps.Add("hasSubmitted", false);
        Debug.Log("added");
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        yield return new WaitForSeconds(1f);

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




    // Check if winning condition is met after each round
    private void ScoreCheck(Player player)
    {
        int score = (int)player.CustomProperties["Score"];
        if (score >= numOfPointsToWin)
        {
            state = GameState.Ending;
            EndGame_S(player);
        } 
        else
        {
            // should trigger the next round... 
            TriggerNextRound();
        }
    }


    

    public void PlayAgain()
    {

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
        timerUI.GetComponent<TMP_Text>().text = $"{minutes}:{seconds}";
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSecondsRealtime(1f);

        currTime -= 1;

        if (currTime < 0) 
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
        int numOfPlayers = players.Length;
        Player[] downloadPlayer = new Player[numOfPlayers - 1]; // exclude the host
        int index = 0;

        for(int i = 0; i < numOfPlayers; i++)
        {
            // nd to exclude host
            if (i != currHost)
            {
                downloadPlayer[index] = players[i];
                index++;
            }
        }

        return downloadPlayer;
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
        // change back to original settings
        judgingUI.SetActive(false);
        timerUI.SetActive(true);
        state = GameState.HostPlaying;
        // reset the submission status
        StartCoroutine(CoResetSubmissionStatus());

        // redirect normal players to waiting room and host to host room
        // redirect the players
        if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[currHost])
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

        if (currTime <= 0)
        {
            if (state == GameState.HostPlaying)
            {
                // if the host did not manage to input in time, automatically skipped.
                OnHostSkip();
            }
            else if (state == GameState.PlayerPlaying)
            {
                // player ran out of time to draw, force submit
                if (!(bool)PhotonNetwork.LocalPlayer.CustomProperties["hasSubmitted"] 
                    && PhotonNetwork.LocalPlayer != players[currHost])
                {
                    // if hasnt submit and not the host
                    OnSubmit();
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
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void OnWordConfirmation_R(object[] data)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // stop the host countdown timer
            InitializeTimer(timeLimit);
        }

        string wordToDisplay = (string)data[0];

        // transition drawing players to drawingUI
        if (PhotonNetwork.LocalPlayer != PhotonNetwork.PlayerList[currHost])
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
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void OnSubmitToggle_R(object[] data)
    {
        Player player = (Player)data[0];
        //change scoreboard toggle => to be implemented after implementing scoreboard behaviour
        scoreboard.ToggleSubmissionStatus(player);
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
        if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[currHost])
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
        object[] package = new object[] { winnerIndex };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.SetWinner,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void SetWinnerOfThisRound_R(object[] data)
    {
        int winnerIndex = (int)data[0];
        StartCoroutine(CoSetWinner(winnerIndex));
    }

    private IEnumerator CoSetWinner(int winnerIndex)
    {
        // display the winning drawing
        judgingUI.GetComponent<DrawingGallery>().LoadDrawing(winnerIndex);

        Player[] drawingPlayers = GetArrayOfPlayersWithoutHost();

        winnerNameText.GetComponent<TMP_Text>().text = drawingPlayers[winnerIndex].NickName + " !"; // set to player name
        
        // pop up something to show that this is the winner for this round
        winnerPanel.SetActive(true);

        yield return new WaitForSeconds(2f);

        winnerPanel.SetActive(false);

        // set score
        yield return StartCoroutine(scoreboard.CoIncrementScore(drawingPlayers[winnerIndex]));

        // check if there is a winner otherwise
        ScoreCheck(drawingPlayers[winnerIndex]);

    }

    public void SetNextHost_S()
    {
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.SetNextHost,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void SetNextHost_R() 
    {
        int numOfPlayers = PhotonNetwork.PlayerList.Length;
        if (currHost >= (numOfPlayers - 1))
        {
            currHost = 0;
        }
        else
        {
            currHost += 1;
        }
        Debug.Log(currHost + " " + players[currHost].NickName);
    }

    public void EndGame_S(Player player)
    {
        object[] package = new object[] { player };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.EndGame,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    private void EndGame_R(object[] data)
    {
        Player player = (Player)data[0];

        judgingUI.SetActive(false);
        endingUI.SetActive(true);
        Debug.Log("Game Ended");

        // display winner
        gameWinnerText.GetComponent<TMP_Text>().text = player.NickName + " !";
        Debug.Log(player.NickName + " wins !");
    }

    #endregion
}
