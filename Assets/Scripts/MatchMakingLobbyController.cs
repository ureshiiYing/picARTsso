using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchMakingLobbyController : MonoBehaviourPunCallbacks
{
    // declare variables in the panels involved in matchmaking
   
    [SerializeField]
    private GameObject mainPanel;
    [SerializeField]
    private GameObject lobbyConnectButton;
    [SerializeField]
    private InputField playerNameInput;
    [SerializeField]
    private GameObject lobbyPanel;

    private string joinRoomName;
    private string createRoomName;
    private int createRoomSize;
    private bool isRoomPublic;

    private List<RoomInfo> roomListings; // list of current rooms
    [SerializeField]
    private Transform roomsContainer; // container for holding all roomsListings
    [SerializeField]
    private GameObject roomListingPrefab; // prefab to display each room

    // called automatically after connecting to server
    // playerprefs caches previously keyed username
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        lobbyConnectButton.SetActive(true);
        roomListings = new List<RoomInfo>();
        isRoomPublic = true;
        createRoomSize = 3;

        // check if player keyed in a name, else generate one for them
        if(PlayerPrefs.HasKey("Nickname"))
        {
            if(PlayerPrefs.GetString("Nickname") == "")
            {
                PhotonNetwork.NickName = "Player" + Random.Range(0, 1000);
            }
            else
            {
                PhotonNetwork.NickName = PlayerPrefs.GetString("Nickname");
            }
        }
        else
        {
            PhotonNetwork.NickName = "Player" + Random.Range(0, 1000);
        }

        playerNameInput.text = PhotonNetwork.NickName;
    }

    // get and set player name, linked to playernameinput
    public void PlayerNameUpdate(string nameInput)
    {
        PhotonNetwork.NickName = nameInput;
        PlayerPrefs.SetString("Nickname", nameInput);
    }

    //enter lobby
    public void JoinGameOnClick() // paired to join game button
    {
        mainPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        PhotonNetwork.JoinLobby(); // joins lobby and receive updates on rooms
    }

    // clear list of rooms displayed
    void ClearRoomListings()
    {
        for (int i = roomsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(roomsContainer.GetChild(i).gameObject);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListings();
        foreach (RoomInfo room in roomList) // loops through each room
        {

            if (room.PlayerCount > 0)
            {
                roomListings.Add(room);
                ListRoom(room);
            }

        }
    }

    // relist rooms
    void ListRoom(RoomInfo room)
    {
        if (room.IsOpen && room.IsVisible)
        {
            GameObject tempListing = Instantiate(roomListingPrefab, roomsContainer);
            RoomButton tempButton = tempListing.GetComponent<RoomButton>();
            tempButton.SetRoom(room.Name, room.MaxPlayers, room.PlayerCount);
        }
    }

    // linked to joinroomname input
    public void OnJoinRoomNameChanged(string nameIn)
    {
        joinRoomName = nameIn;
    }

    public void JoinRoom()
    {
        if (joinRoomName != "")
        {
            Debug.Log("Joining room now");
            PhotonNetwork.JoinRoom(joinRoomName);
    
        }
        else
        {
            Debug.Log("Please enter a valid input for room name!");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Tried to join a room but failed, room is non-existent, closed or full.");
    }

    // linked to public toggle
    public void OnPublicChanged(bool boolean)
    {
        Debug.Log("toggle is " + boolean);
        isRoomPublic = boolean;
    }

    // linked to createroomname input
    public void OnCreateRoomNameChanged(string nameIn)
    {
        createRoomName = nameIn;
    }

    // linked to dropdown bar selecting size of room
    public void OnRoomSizeChanged(int val)
    {
        createRoomSize = val + 3;
    }

    public void CreateRoom()
    {

        Debug.Log("Creating room now");
        RoomOptions roomOps = new RoomOptions() { IsVisible = isRoomPublic, IsOpen = true, MaxPlayers = (byte)createRoomSize };
        roomOps.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        roomOps.CustomRoomProperties.Add("MaxPoints", 5);
        roomOps.CustomRoomProperties.Add("Theme", "GeneralTheme");
        roomOps.CustomRoomProperties.Add("TimeLimit", 30);
        PhotonNetwork.CreateRoom(createRoomName, roomOps);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Tried to create a new room but failed, there must already be a room with same name.");
    }

    // return to lobby
    public void MatchmakingCancel()
    {
        mainPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        PhotonNetwork.LeaveLobby();
    }

}
 