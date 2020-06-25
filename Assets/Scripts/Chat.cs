using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Chat;
using Photon.Pun;

public class Chat : MonoBehaviour, IChatClientListener
{
    public ChatClient chatClient;

    private string userID;
    [SerializeField]
    private InputField messageInput;
    private string channelName;
    [SerializeField]
    private GameObject chatPanel;

    [SerializeField]
    private Transform messageContainer;
    [SerializeField]
    private GameObject messageListingPrefab; // gameobject containing text component
    private int maxMessages = 100;

    [SerializeField] private GameObject noti;

    // has yet to implement:
    // 1) limit number of messages stored in chat
    // 2) unsubscribe and disconnected from chat (retry connection?)
    // connect() -> onconnected() -> onsubscribed (fn call sequence)

    public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message)
    {
        return;
    }

    public void OnChatStateChange(ChatState state)
    {
        Debug.Log(state.ToString());
    }

    // when connected to server, subscribe to chat channel of room
    public void OnConnected()
    {
        chatClient.Subscribe(new string[] {channelName}); //subscribe to chat channel once connected to server
    }

    public void OnDisconnected()
    {
        Debug.Log("You have been disconnected!");
    }

    // updates chat display box
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
           
        for (int i = 0; i < senders.Length; i++)
        {
            if (messageContainer.childCount >= maxMessages)
            {
                Destroy(messageContainer.GetChild(0).gameObject); //destroyyyy oldest message and make way for new ones
            }
            GameObject tempListing = Instantiate(messageListingPrefab, messageContainer);
            Text tempText = tempListing.transform.GetComponent<Text>();
            tempText.text = string.Format("{2} ", "", senders[i], messages[i]); //wtfisthis
        }

        // noti icon spawn
        if (!chatPanel.activeSelf)
        {
            noti.SetActive(true);
        }
    }

    public void ClearNoti()
    {
        noti.SetActive(false);
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        throw new System.NotImplementedException();
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new System.NotImplementedException();
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        if (results[0])
        {
            Debug.Log("Subscribed to channel: " + channels[0]);
        }
        else
        {
            Debug.Log("failed to join channel");
        }
    }

    public void OnUnsubscribed(string[] channels)
    {
        Debug.Log("Unsubscribed from channel: " + channelName);
    }

    public void OnUserSubscribed(string channel, string user)
    {
        Debug.Log(user + " subscribed to channel: " + channelName);
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        Debug.Log(user + " unsubscribed from channel: " + channelName);
    }

    // connect to chat server
    public void Connect()
    {
        // initialise variables
        // connect to chat server
        this.chatClient = new ChatClient(this);
        this.userID = PhotonNetwork.NickName;
        this.channelName = PhotonNetwork.CurrentRoom.Name;
        this.chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat,
            PhotonNetwork.AppVersion, new AuthenticationValues(userID));

    }

    public void SendMessageOnClick()
    {
        if (messageInput.text != "")
        {
            this.chatClient.PublishMessage(channelName, userID + ": " + messageInput.text);
            this.messageInput.text = "";
            Debug.Log("sent message");
        }
    }

    public void OnEnterSend()
    {
        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
        {
            if (messageInput.text != "")
            {
                this.chatClient.PublishMessage(channelName, userID + ": " + messageInput.text);
                this.messageInput.text = "";
                Debug.Log("sent message");
            }

        }
    }

    void Start()
    {
        Connect();
    }

    // Update is called once per frame
    void Update()
    {
        //maintain the chat connection
        if (chatClient != null)
        {
            chatClient.Service();
        }
    }

}
