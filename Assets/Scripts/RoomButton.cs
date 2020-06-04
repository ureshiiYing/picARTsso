using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text sizeText;

    private string roomName;
    private int roomSize;
    private int playerCount;

    public void JoinRoomOnClick()
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void SetRoom(string nameIn, int sizeIn, int countIn)
    {
        roomName = nameIn;
        roomSize = sizeIn;
        playerCount = countIn;
        nameText.text = nameIn;
        sizeText.text = countIn + "/" + sizeIn;
    }
}
