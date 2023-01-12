using Fusion;
using Fusion.XR.Host.Rig;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Host.Grabbing
{
    // Store the info describbing a grabbing state
    public struct GrabInfo : INetworkStruct
    {
        public NetworkBehaviourId grabbedObjectId;
        public Vector3 localPositionOffset;
        public Quaternion localRotationOffset;
        // We want the local user accurate ungrab position to be enforced on the network, and so shared in the input (to avoid the grabbable following "too long" the grabber)
        public Vector3 ungrabPosition;
        public Quaternion ungrabRotation; 
        public Vector3 ungrabVelocity;
        public Vector3 ungrabAngularVelocity;
    }

    /**
     * 
     * Allows a NetworkHand to grab NetworkGrabbable objects
     * 
     **/

    [RequireComponent(typeof(NetworkHand))]
    [OrderAfter(typeof(NetworkHand))]
    public class NetworkGrabber : NetworkBehaviour
    {
        [Networked]
        public GrabInfo GrabInfo { get; set; }

        NetworkGrabbable grabbedObject;
        public NetworkTransform networkTransform;
        public NetworkHand hand;
        GrabInfo previousGrab;

        private void Awake()
        {
            networkTransform = GetComponent<NetworkTransform>();
            hand = GetComponentInParent<NetworkHand>();
        }

        public override void Spawned()
        {
            base.Spawned();
            if (hand.IsLocalNetworkRig)
            {
                hand.LocalHardwareHand.grabber.networkGrabber = this;
            }
        }
        
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (Runner.IsForward)
            {
                HandleGrabInfoChange(previousGrab, GrabInfo);
                previousGrab = GrabInfo;
            }
        }

        void HandleGrabInfoChange(GrabInfo previousGrabInfo, GrabInfo newGrabInfo)
        {
            if (previousGrabInfo.grabbedObjectId !=  newGrabInfo.grabbedObjectId)
            {
                if (grabbedObject != null)
                {
                    grabbedObject.Ungrab(this, newGrabInfo);
                    grabbedObject = null;
                }
                // We have to look for the grabbed object has it has changed
                NetworkGrabbable newGrabbedObject;

                // If an object is grabbed, we look for it through the runner with its Id
                if (newGrabInfo.grabbedObjectId != NetworkBehaviourId.None && Object.Runner.TryFindBehaviour(newGrabInfo.grabbedObjectId, out newGrabbedObject))
                {
                    grabbedObject = newGrabbedObject;
                    if (grabbedObject != null)
                    {
                        grabbedObject.Grab(this, newGrabInfo);
                    }
                }
            }
        }
    }
}
