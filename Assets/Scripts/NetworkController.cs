using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkController : MonoBehaviourPunCallbacks
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // connect to master server
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("we are connected to master server" + PhotonNetwork.CloudRegion + " :)!"); 
    }
}
