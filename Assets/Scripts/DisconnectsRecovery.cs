using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Photon.Pun.UtilityScripts
{
    /// <summary>
    /// Unexpected disconnects recovery
    /// </summary>
    public class DisconnectsRecovery : MonoBehaviourPunCallbacks
    {
        [Tooltip("Whether or not attempt a rejoin without doing any checks.")]
        [SerializeField]
        private bool skipRejoinChecks;
        [Tooltip("Whether or not realtime webhooks are configured with persistence enabled")]
        [SerializeField]
        private bool persistenceEnabled;

        private bool rejoinCalled;

        private int minTimeRequiredToRejoin = 60000; // TODO: set dynamically based on PhotonNetwork.NetworkingClient.LoadBalancingPeer.RoundTripTime

        private DisconnectCause lastDisconnectCause;
        public bool wasInRoom;

        private bool reconnectCalled;

        private bool pendingAction;

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.NetworkingClient.StateChanged += this.OnStateChanged;
           
            //PhotonNetwork.KeepAliveInBackground = 0f;
            //PhotonNetwork.PhotonServerSettings.RunInBackground = false;
            //Application.runInBackground = false;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.NetworkingClient.StateChanged -= this.OnStateChanged;
        }


        private void OnStateChanged(ClientState fromState, ClientState toState)
        {
            if (toState == ClientState.Disconnected)
            {
                Debug.LogFormat("OnStateChanged from {0} to {1}, PeerState={2}", fromState, toState,
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.PeerState);

                //Debug.Log("Pending action raised");
                pendingAction = true;

            }
        }


        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogFormat("OnDisconnected(cause={0}) ClientState={1} PeerState={2}",
                cause,
                PhotonNetwork.NetworkingClient.State,
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.PeerState);
            if (rejoinCalled)
            {
                //Debug.Log("Rejoin failed, client disconnected");
                rejoinCalled = false;
                return;
            }

            if (reconnectCalled)
            {
                //Debug.Log("Reconnect failed, client disconnected");
                reconnectCalled = false;
                return;
            }
            lastDisconnectCause = cause;
            if (PhotonNetwork.NetworkingClient.State == ClientState.Disconnected)
            {
                //Debug.Log("Pending action raised");
                pendingAction = true;
            }
        }

        private void Start()
        {
            InvokeRepeating("CheckAction", 0, 1f);
            wasInRoom = PhotonNetwork.CurrentRoom != null;
        }

        private void CheckAction()
        {
            if (pendingAction)
            {
                pendingAction = false;
                //Debug.Log("handle disconnect now");
                this.HandleDisconnect();
            }
        }


        private void HandleDisconnect()
        {
            Debug.Log(lastDisconnectCause.ToString());
            //switch (lastDisconnectCause)
            //{
            //    case DisconnectCause.Exception:
            //    case DisconnectCause.ServerTimeout:
            //    case DisconnectCause.ClientTimeout:
            //    case DisconnectCause.DisconnectByServerLogic:
            //    case DisconnectCause.AuthenticationTicketExpired:
            //    case DisconnectCause.DisconnectByServerReasonUnknown:

            //        break;
            //    case DisconnectCause.OperationNotAllowedInCurrentState:
            //    case DisconnectCause.CustomAuthenticationFailed:
            //    case DisconnectCause.DisconnectByClientLogic:
            //    case DisconnectCause.InvalidAuthentication:
            //    case DisconnectCause.ExceptionOnConnect:
            //    case DisconnectCause.MaxCcuReached:
            //    case DisconnectCause.InvalidRegion:
            //    case DisconnectCause.None:
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException("cause", lastDisconnectCause, null);
            //}
            if (wasInRoom)
            {
                //Debug.Log("CheckAndRejoin called");
                this.CheckAndRejoin();
            }
            else
            {
                //Debug.Log("PhotonNetwork.Reconnect called");
                reconnectCalled = PhotonNetwork.Reconnect();
            }
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            if (!rejoinCalled)
            {
                return;
            }
            rejoinCalled = false;    
            Debug.LogFormat("Quick rejoin failed with error code: {0} & error message: {1}", returnCode, message);
            StartCoroutine(RejoinRoomFailed(message));
        }

        // insert kicked from game notice for being idle for too long, returns to menu scene promptly after 3 seconds
        private IEnumerator RejoinRoomFailed(string message)
        {
            FindObjectOfType<ErrorMessagesHandler>().DisplayError(
                string.Format("Failed to rejoin room: {0}. Please rejoin game.", message));
            yield return new WaitForSecondsRealtime(3.0f);
            SceneManager.LoadScene(0);
        }

        public override void OnJoinedRoom()
        {
            if (rejoinCalled)
            {
                Debug.Log("Rejoin successful");
                rejoinCalled = false;
            }
        }

        private void CheckAndRejoin()
        {
            if (skipRejoinChecks)
            {
                //Debug.Log("PhotonNetwork.ReconnectAndRejoin called");
                rejoinCalled = PhotonNetwork.ReconnectAndRejoin();
            }
            else
            {
                // idk what this is i just copypasted this whole page and modified
                //bool wasLastActivePlayer = true;
                //if (!persistenceEnabled)
                //{
                //    for (int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++)
                //    {
                //        if (!PhotonNetwork.PlayerListOthers[i].IsInactive)
                //        {
                //            wasLastActivePlayer = false;
                //            break;
                //        }
                //    }
                //}
                //if ((PhotonNetwork.CurrentRoom.PlayerTtl < 0 || PhotonNetwork.CurrentRoom.PlayerTtl > minTimeRequiredToRejoin) // PlayerTTL checks
                //  && (!wasLastActivePlayer || PhotonNetwork.CurrentRoom.EmptyRoomTtl > minTimeRequiredToRejoin || persistenceEnabled)) // EmptyRoomTTL checks
                //{
                //    Debug.Log("PhotonNetwork.ReconnectAndRejoin called");
                //    rejoinCalled = PhotonNetwork.ReconnectAndRejoin();
                //}
                //else
                //{
                //    Debug.Log("PhotonNetwork.ReconnectAndRejoin not called, PhotonNetwork.Reconnect is called instead.");
                //    reconnectCalled = PhotonNetwork.Reconnect();
                //}
            }
        }

        public override void OnConnectedToMaster()
        {
            if (reconnectCalled)
            {
                Debug.Log("Reconnect successful");
                reconnectCalled = false;
            }
        }
        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                pendingAction = true;
                PhotonNetwork.Disconnect();
            }
        }

    }
}