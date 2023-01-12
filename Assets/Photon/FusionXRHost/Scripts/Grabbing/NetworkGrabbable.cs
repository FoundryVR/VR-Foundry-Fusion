using Fusion.XR.Host.Rig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Fusion.XR.Host.Grabbing
{
    [OrderAfter(typeof(NetworkGrabber), typeof(NetworkHand), typeof(NetworkRigidbody), typeof(NetworkTransform))]
    public abstract class NetworkGrabbable : NetworkBehaviour
    {
        public NetworkGrabber currentGrabber;
        protected NetworkGrabber lastGrabber = null;
        public bool IsGrabbed => currentGrabber != null;
        [HideInInspector]
        public NetworkTransform networkTransform = null;

        [Header("Events")]
        public UnityEvent onDidUngrab = new UnityEvent();
        public UnityEvent<NetworkGrabber> onDidGrab = new UnityEvent<NetworkGrabber>();
        
        public abstract void Grab(NetworkGrabber newGrabber, GrabInfo newGrabInfo);
        public abstract void Ungrab(NetworkGrabber grabber, GrabInfo newGrabInfo);

        public void DidGrab()
        {
            if (onDidGrab != null) onDidGrab.Invoke(currentGrabber);
        }

        public void DidUngrab()
        {
            if (onDidGrab != null) onDidGrab.Invoke(lastGrabber);
        }
    }
}


