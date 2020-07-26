using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class MatchMakingLobbyController : MonoBehaviourPunCallbacks
{
    // declare variables in the panels involved in matchmaking
   
    [SerializeField]
    private GameObject mainPanel;
    [SerializeField]
    private GameObject startGameButton;
    [SerializeField]
    private InputField playerNameInput;
    [SerializeField]
    private GameObject lobbyPanel;
    [SerializeField]
    private GameObject createPanel;
    [SerializeField]
    private GameObject choosePanel;
    [SerializeField]
    private GameObject errorPanel;
    [SerializeField]
    private GameObject passwordPanel;
    [SerializeField]
    private TMP_InputField joinPassword;
    [SerializeField]
    private TMP_InputField createPassword;
    [SerializeField]
    private Toggle isPrivateToggle;


    private string joinRoomName;

    private string createRoomName;
    private int createRoomSize;
    private bool isRoomPrivate;
    private string password;
    private RoomInfo roomToBeJoined;

    private List<RoomInfo> roomListings; // list of current rooms
    [SerializeField]
    private Transform roomsContainer; // container for holding all roomsListings
    [SerializeField]
    private GameObject roomListingPrefab; // prefab to display each room

    // called automatically after connecting to server
    // playerprefs caches previously keyed username
    public override void OnConnectedToMaster()
    {
        Debug.Log("Joined Master Server.");
        PhotonNetwork.AutomaticallySyncScene = true;
        startGameButton.SetActive(true);
        roomListings = new List<RoomInfo>();
        isRoomPrivate = false;
        isPrivateToggle.SetIsOnWithoutNotify(false);
        createPassword.text = "";
        createPassword.gameObject.SetActive(false);
        createRoomSize = 3;
        joinRoomName = "";
        password = "";

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
    public void JoinLobbyOnClick() // paired to join existing room button
    {
        choosePanel.SetActive(false);
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
            tempButton.SetRoom(room);
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby.");
    }

    // linked to joinroomname input
    public void OnJoinRoomNameChanged(string nameIn)
    {
        joinRoomName = nameIn.Trim();
    }

    public void StartJoinRoomProcedure(string roomName)
    {
        joinRoomName = roomName;
        StartJoinRoomProcedure();
    }


    public void StartJoinRoomProcedure()
    {
        if (joinRoomName != "")
        {
            foreach (RoomInfo room in roomListings) // loops through each room to get room info
            {
                if (room.Name == joinRoomName)
                {
                    roomToBeJoined = room;
                    break;
                }
            }

            //check if pw protected
            if (roomToBeJoined != null && (bool) roomToBeJoined.CustomProperties["IsPrivate"])
            {
                passwordPanel.SetActive(true);
            }
            else
            {
                Debug.Log("Joining room now");
                PhotonNetwork.JoinRoom(joinRoomName);
            }

        }
        else
        {
            errorPanel.GetComponent<ErrorMessagesHandler>().DisplayError("Please enter a valid input for room name!");
        }
    }

    public void AuthJoinRoomOnClick()
    {
        if (joinPassword.text == (string)roomToBeJoined.CustomProperties["Password"])
        {
            PhotonNetwork.JoinRoom(roomToBeJoined.Name);
            joinPassword.text = "";
            passwordPanel.SetActive(false);
        }
        else
        {
            FindObjectOfType<ErrorMessagesHandler>().DisplayError("Incorrect password!");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        errorPanel.GetComponent<ErrorMessagesHandler>().DisplayError("Tried to join a room but failed, room is non-existent, closed or full.");
    }

    // linked to public toggle
    public void OnPrivateChanged(bool boolean)
    {
        isRoomPrivate = boolean;
        createPassword.gameObject.SetActive(boolean);
    }

    // linked to create password inputfield
    public void OnCreatePasswordChanged(string passIn)
    {
        string temp = passIn.Replace(" ", string.Empty); // remove all spaces in pw
        password = temp;
    }

    // linked to createroomname input
    public void OnCreateRoomNameChanged(string nameIn)
    {
        string temp = nameIn.Trim();
        if (temp != "")
        {
            createRoomName = temp;
        }
    }

    // linked to dropdown bar selecting size of room
    public void OnRoomSizeChanged(int val)
    {
        createRoomSize = val + 3;
    }

    public void CreateRoom()
    {
        if (isRoomPrivate && password == "")
        {
            errorPanel.GetComponent<ErrorMessagesHandler>().DisplayError("Please enter a valid password.");
            return;
        }
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)createRoomSize };
        roomOps.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        roomOps.CustomRoomProperties.Add("MaxPoints", 3);
        roomOps.CustomRoomProperties.Add("Theme", "GeneralTheme");
        roomOps.CustomRoomProperties.Add("TimeLimit", 30);
        roomOps.CustomRoomProperties.Add("IsPrivate", isRoomPrivate);
        roomOps.CustomRoomProperties.Add("Password", password);
        PhotonNetwork.CreateRoom(createRoomName, roomOps);
        Debug.Log("Creating room now");
    }

    public override void OnCreatedRoom()
    {
        PhotonNetwork.CurrentRoom.SetPropertiesListedInLobby(new string[] { "Password", "IsPrivate" });
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorPanel.GetComponent<ErrorMessagesHandler>().DisplayError("Tried to create a new room but failed, there must already be a room with same name.");
    }

    // return to choose or create
    public void LobbyCancel()
    {
        choosePanel.SetActive(true);
        lobbyPanel.SetActive(false);
        PhotonNetwork.LeaveLobby();
    }

}
 