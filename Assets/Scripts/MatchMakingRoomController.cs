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
    private GameObject roomPanel;
    [SerializeField]
    private GameObject waitPanel;

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
        roomPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        roomsNameDisplay.text = PhotonNetwork.CurrentRoom.Name;
        if (PhotonNetwork.IsMasterClient)
        {
            //redundant tbh
            startButton.SetActive(true);
            timeLimit.SetActive(true);
            selectTheme.SetActive(true);
            numOfPoints.SetActive(true);
            timeLimitLabel.SetActive(true);
            selectThemeLabel.SetActive(true);
            numOfPointsLabel.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
            timeLimit.SetActive(false);
            selectTheme.SetActive(false);
            numOfPoints.SetActive(false);
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
            timeLimit.SetActive(true);
            selectTheme.SetActive(true);
            numOfPoints.SetActive(true);
            timeLimitLabel.SetActive(true);
            selectThemeLabel.SetActive(true);
            numOfPointsLabel.SetActive(true);
        }
        
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
        yield return new WaitForSeconds(1);
        PhotonNetwork.JoinLobby();
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


