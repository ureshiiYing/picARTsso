using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReportController : MonoBehaviourPunCallbacks //, IOnEventCallback
{
    public ArrayList reportedPlayers;

    // because onLeftRoom callback will be called if disconnected
    private bool leaveRoomDueToReport = false;
    //private byte KickPlayer = 1;

    [SerializeField]
    private GameObject errorPanel;

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    private void Start()
    {
        reportedPlayers = new ArrayList();
    }

    public void AddVote(Player player)
    {
        if (reportedPlayers.Contains(player))
        {
            // replace with user messsage
            errorPanel.GetComponent<ErrorMessagesHandler>().DisplayError("Already reported this player.");
        }
        else
        {
            StartCoroutine(CoAddVote(player));
        }

    }

    private IEnumerator CoAddVote(Player player)
    {
        // create chat message to see who reported who
        FindObjectOfType<Chat>().chatClient.PublishMessage(PhotonNetwork.CurrentRoom.Name,
            PhotonNetwork.LocalPlayer.NickName + " has voted to kick " + player.NickName + ".");
        ExitGames.Client.Photon.Hashtable playerOps = new ExitGames.Client.Photon.Hashtable();
        int numReported = (int)player.CustomProperties["ReportCount"] + 1;
        playerOps.Add("ReportCount", numReported);
        player.SetCustomProperties(playerOps);

        yield return new WaitForSeconds(1f);

        reportedPlayers.Add(player);

        //this.gameObject.SetActive(false);
        StartCoroutine(CoCheckReported(player));
    }

    // check if theres enough votes to kickkk
    private IEnumerator CoCheckReported(Player player)
    {

        if ((int)player.CustomProperties["ReportCount"] > (int)PhotonNetwork.PlayerList.Length / 2)
        {
            //KickPlayer_S(player);
            ExitGames.Client.Photon.Hashtable playerOps = new ExitGames.Client.Photon.Hashtable();
            playerOps.Add("IsKicked", true);
            player.SetCustomProperties(playerOps);

            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("KickPlayer", player);
        }
        yield return new WaitForSecondsRealtime(1f);
    }

    [PunRPC]
    private void KickPlayer()
    {
        leaveRoomDueToReport = true;
        PhotonNetwork.LeaveRoom(); // load lobby scene, returns to master server
    }

    //public void OnEvent(EventData photonEvent)
    //{
    //    byte eventCode = photonEvent.Code;
    //    object[] data = (object[])photonEvent.CustomData;
    //    Debug.Log("received event");

    //    if (eventCode == KickPlayer)
    //    {
    //        KickPlayer_R((Player)data[0]);
    //    }
    //}

    //public void KickPlayer_S(Player player)
    //{
    //    Debug.Log("kickplayer raised");
    //    object[] package = new object[] { player };

    //    PhotonNetwork.RaiseEvent(
    //        KickPlayer,
    //        package,
    //        new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient, CachingOption = EventCaching.AddToRoomCache },
    //        new SendOptions { Reliability = true }
    //    );
    //}

    //public void KickPlayer_R(Player player)
    //{
    //    Debug.Log("kickplayer called");
    //    PhotonNetwork.CloseConnection(player);
    //    Debug.Log(player.NickName + " has been kicked");
    //}



    public override void OnLeftRoom()
    {
        if (leaveRoomDueToReport)
        {
            // message on chat to say theyre kicked
            StartCoroutine(KickedFromGame());
        }

    }

    // can abstract but...hm
    // insert kicked from game notice for being toxic, returns to menu scene promptly after 3 seconds
    private IEnumerator KickedFromGame()
    {
        FindObjectOfType<ErrorMessagesHandler>().DisplayError(
            string.Format("You have been reported and kicked from the game."));
        yield return new WaitForSecondsRealtime(3.0f);
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer.IsInactive)
        {
            if ((bool) otherPlayer.CustomProperties["IsKicked"])
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    FindObjectOfType<Chat>().chatClient.PublishMessage(PhotonNetwork.CurrentRoom.Name,
                    otherPlayer.NickName + " has been kicked.");
                }
                StartCoroutine(FindObjectOfType<Scoreboard>().CoRefresh());
            }
            else
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    FindObjectOfType<Chat>().chatClient.PublishMessage(PhotonNetwork.CurrentRoom.Name,
                    otherPlayer.NickName + " has disconnected.");
                }
            }
        }
        else if (!PhotonNetwork.CurrentRoom.Players.ContainsValue(otherPlayer))
        {
            if (!(bool)otherPlayer.CustomProperties["IsKicked"])
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    FindObjectOfType<Chat>().chatClient.PublishMessage(PhotonNetwork.CurrentRoom.Name,
                    otherPlayer.NickName + " has left the room.");
                }
                
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            FindObjectOfType<Chat>().chatClient.PublishMessage(PhotonNetwork.CurrentRoom.Name,
            newPlayer.NickName + " has reconnected.");
        }
    }

}
