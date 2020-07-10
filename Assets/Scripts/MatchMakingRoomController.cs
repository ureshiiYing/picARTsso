using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchMakingRoomController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private int multiplayerSceneIndex;


    [SerializeField]
    private GameObject lobbyPanel;
    [SerializeField]
    private GameObject createPanel;
    [SerializeField]
    private GameObject roomPanel;
    [SerializeField]
    private GameObject joinPanel;

    [SerializeField]
    private GameObject startButton;

    [SerializeField]
    private GameObject timeLimit;
    [SerializeField]
    private GameObject selectTheme;
    [SerializeField]
    private GameObject numOfPoints;
    [SerializeField]
    private GameObject timeLimitLabel;
    [SerializeField]
    private GameObject selectThemeLabel;
    [SerializeField]
    private GameObject numOfPointsLabel;

    [SerializeField]
    private Transform playerContainer;
    [SerializeField]
    private GameObject playerListingPrefab;

    [SerializeField]
    private Text roomsNameDisplay;

    // clear list of players displayed
    void ClearPlayerListings()
    {
        for (int i = playerContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(playerContainer.GetChild(i).gameObject);
        }
    }

    // relist players
    void ListPlayers()
    {
        // creates a listing for each player
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject tempListing = Instantiate(playerListingPrefab, playerContainer);
            Text tempText = tempListing.transform.GetChild(0).GetComponent<Text>();
            tempText.text = player.NickName;
        }
    }

    // reload list of players & change panels
    public override void OnJoinedRoom()
    {
        createPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        joinPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomsNameDisplay.text = PhotonNetwork.CurrentRoom.Name;
        if (PhotonNetwork.IsMasterClient)
        {
            //redundant tbh
            startButton.SetActive(true);
            timeLimit.gameObject.SetActive(true);
            selectTheme.gameObject.SetActive(true);
            numOfPoints.gameObject.SetActive(true);
            timeLimitLabel.SetActive(true);
            selectThemeLabel.SetActive(true);
            numOfPointsLabel.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
            timeLimit.gameObject.SetActive(false);
            selectTheme.gameObject.SetActive(false);
            numOfPoints.gameObject.SetActive(false);
            timeLimitLabel.SetActive(false);
            selectThemeLabel.SetActive(false);
            numOfPointsLabel.SetActive(false);
        }
        ClearPlayerListings();
        ListPlayers();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        ClearPlayerListings();
        ListPlayers();
    }

    // allow new master client to change attributes of game and start game
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ClearPlayerListings();
        ListPlayers();
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
            timeLimit.gameObject.SetActive(true);
            selectTheme.gameObject.SetActive(true);
            numOfPoints.gameObject.SetActive(true);
            timeLimitLabel.SetActive(true);
            selectThemeLabel.SetActive(true);
            numOfPointsLabel.SetActive(true);
        }
        
    }

    // linked to dropdown bar selecting time limit of room, updates custom room property
    public void OnTimeLimitChanged(int val)
    {
        ExitGames.Client.Photon.Hashtable newRoomOps = new ExitGames.Client.Photon.Hashtable();
        switch (val)
        {
            case 0:
                newRoomOps.Add("TimeLimit", 30);
                break;
            case 1:
                newRoomOps.Add("TimeLimit", 60);
                break;
            case 2:
                newRoomOps.Add("TimeLimit", 90);
                break;
            case 3:
                newRoomOps.Add("TimeLimit", 120);
                break;
            case 4:
                newRoomOps.Add("TimeLimit", 180);
                break;
            case 5:
                newRoomOps.Add("TimeLimit", 300);
                break;
            default:
                Debug.Log("nothing is selected for time limit...");
                break;

        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(newRoomOps);
    }

    // linked to dropdown bar selecting max points of room, updates custom room property
    public void OnMaxPointsChanged(int val)
    {
        ExitGames.Client.Photon.Hashtable newRoomOps = new ExitGames.Client.Photon.Hashtable();
        switch (val)
        {
            case 0:
                newRoomOps.Add("MaxPoints", 5);
                break;
            case 1:
                newRoomOps.Add("MaxPoints", 10);
                break;
            case 2:
                newRoomOps.Add("MaxPoints", 15);
                break;
            case 3:
                newRoomOps.Add("MaxPoints", 20);
                break;
            default:
                Debug.Log("nothing is selected for max points...");
                break;

        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(newRoomOps);
    }

    // linked to dropdown bar selecting theme of room, updates custom room property
    public void OnThemeChanged(int val)
    {
        ExitGames.Client.Photon.Hashtable newRoomOps = new ExitGames.Client.Photon.Hashtable();
        switch (val)
        {
            case 0:
                newRoomOps.Add("Theme", "GeneralTheme");
                break;
            case 1:
                newRoomOps.Add("Theme", "AnimalsTheme");
                break;
            case 2:
                newRoomOps.Add("Theme", "FoodTheme");
                break;
            case 3:
                newRoomOps.Add("Theme", "ObjectsTheme");
                break;
            case 4:
                newRoomOps.Add("Theme", "SportsTheme");
                break;
            default:
                Debug.Log("nothing is selected for theme...");
                break;

        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(newRoomOps);
    }

    // linked to start button
    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(multiplayerSceneIndex);
    }

    IEnumerator rejoinLobby()
    {
        PhotonNetwork.JoinLobby();
        yield return new WaitForSeconds(1);
    }

    public void BackOnClick()
    { 
        lobbyPanel.SetActive(true);
        roomPanel.SetActive(false);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        StartCoroutine(rejoinLobby());

    }
}


