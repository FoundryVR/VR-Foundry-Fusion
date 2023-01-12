using Fusion.Sockets;
using Fusion.XR.Host.Grabbing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Host.Rig
{
    public enum RigPart
    {
        None,
        Headset,
        LeftController,
        RightController,
        Undefined
    }

    // Include all rig parameters in an network input structure
    public struct RigInput : INetworkInput
    {
        public Vector3 playAreaPosition;
        public Quaternion playAreaRotation;
        public Vector3 leftHandPosition;
        public Quaternion leftHandRotation;
        public Vector3 rightHandPosition;
        public Quaternion rightHandRotation;
        public Vector3 headsetPosition;
        public Quaternion headsetRotation;
        public HandCommand leftHandCommand;
        public HandCommand rightHandCommand;
        public GrabInfo leftGrabInfo;
        public GrabInfo rightGrabInfo;
    }

    /**
     * 
     * Hardware rig gives access to the various rig parts: head, left hand, right hand, and the play area, represented by the hardware rig itself
     *  
     * Can be moved, either instantanesously, or with a camera fade
     * 
     **/

    public class HardwareRig : MonoBehaviour, INetworkRunnerCallbacks
    {
        public HardwareHand leftHand;
        public HardwareHand rightHand;
        public HardwareHeadset headset;
        public NetworkRunner runner;

        private void Start()
        {
            if(runner == null)
            {
                Debug.LogError("Runner has to be set in the inspector to forward the input");
            }
            runner.AddCallbacks(this);

        }

        #region Locomotion
        // Update the hardware rig rotation. This will trigger a Riginput network update
        public virtual void Rotate(float angle)
        {
            transform.RotateAround(headset.transform.position, transform.up, angle);
        }
        

        // Update the hardware rig position. This will trigger a Riginput network update
        public virtual void Teleport(Vector3 position)
        {
            Vector3 headsetOffet = headset.transform.position - transform.position;
            headsetOffet.y = 0;
            transform.position = position - headsetOffet;
        }

        // Teleport the rig with a fader
        public virtual IEnumerator FadedTeleport(Vector3 position)
        {
            if (headset.fader) yield return headset.fader.FadeIn();
            Teleport(position);
            if (headset.fader) yield return headset.fader.WaitBlinkDuration();
            if (headset.fader) yield return headset.fader.FadeOut();
        }

        // Rotate the rig with a fader
        public virtual IEnumerator FadedRotate(float angle)
        {
            if (headset.fader) yield return headset.fader.FadeIn();
            Rotate(angle);
            if (headset.fader) yield return headset.fader.WaitBlinkDuration();
            if (headset.fader) yield return headset.fader.FadeOut();
        }
        #endregion

        #region INetworkRunnerCallbacks

        // Prepare the input, that will be read by NetworkRig in the FixedUpdateNetwork
        public void OnInput(NetworkRunner runner, NetworkInput input) {
            RigInput rigInput = new RigInput();
            rigInput.playAreaPosition = transform.position; 
            rigInput.playAreaRotation = transform.rotation;
            rigInput.leftHandPosition = leftHand.transform.position;
            rigInput.leftHandRotation = leftHand.transform.rotation;
            rigInput.rightHandPosition = rightHand.transform.position;
            rigInput.rightHandRotation = rightHand.transform.rotation;
            rigInput.headsetPosition = headset.transform.position;
            rigInput.headsetRotation = headset.transform.rotation;
            rigInput.leftHandCommand = leftHand.handCommand;
            rigInput.rightHandCommand = rightHand.handCommand;

            rigInput.leftGrabInfo = leftHand.grabber.GrabInfo;
            rigInput.rightGrabInfo = rightHand.grabber.GrabInfo;

            input.Set(rigInput);
        }

        #endregion

        #region INetworkRunnerCallbacks (unused)
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }


        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnConnectedToServer(NetworkRunner runner) { }

        public void OnDisconnectedFromServer(NetworkRunner runner) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }
        #endregion
    }
}
