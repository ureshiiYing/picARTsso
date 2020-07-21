using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkController : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(PhotonNetwork.NetworkClientState);
        if (PhotonNetwork.NetworkClientState == ClientState.Joined)
        {
            FindObjectOfType<MatchMakingRoomController>().OnJoinedRoom();
        }
        else if (PhotonNetwork.NetworkClientState == ClientState.PeerCreated ||
            PhotonNetwork.NetworkClientState == ClientState.Disconnected)
        {
            PhotonNetwork.ConnectUsingSettings(); // connect to master server
        }
    }
}
