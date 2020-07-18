using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class ReportButton : MonoBehaviourPunCallbacks //, IOnEventCallback
{
    private Player toBeKicked;

    public void OnReportClicked()
    {
        FindObjectOfType<ReportController>().AddVote(toBeKicked);
    }

    

    public void WhenInstantiated(Player player)
    {
        toBeKicked = player;
    }

}
