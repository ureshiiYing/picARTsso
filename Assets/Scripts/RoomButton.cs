using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomButton : MonoBehaviour
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text sizeText;
    [SerializeField]
    private Image isPrivateImage;

    private string roomName;

    public void JoinRoomOnClick()
    {
        FindObjectOfType<MatchMakingLobbyController>().StartJoinRoomProcedure(roomName);
    }


    public void SetRoom(RoomInfo room)
    {
        roomName = room.Name;
        nameText.text = roomName;
        sizeText.text = room.PlayerCount + "/" + room.MaxPlayers;
        isPrivateImage.gameObject.SetActive((bool)room.CustomProperties["IsPrivate"]);
    }
}
