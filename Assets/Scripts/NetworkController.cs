using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkController : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // connect to master server

    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("we are connected to " + PhotonNetwork.CloudRegion + " :)!"); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
