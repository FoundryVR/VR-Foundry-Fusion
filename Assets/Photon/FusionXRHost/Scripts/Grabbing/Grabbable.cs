using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Host.Grabbing
{
    public abstract class Grabbable : MonoBehaviour
    {
        public Grabber currentGrabber;
        [HideInInspector]
        public NetworkGrabbable networkGrabbable = null;
        [HideInInspector]
        public Vector3 localPositionOffset;
        [HideInInspector]
        public Quaternion localRotationOffset;
        [HideInInspector]
        public Vector3 ungrabPosition;
        [HideInInspector]
        public Quaternion ungrabRotation;
        [HideInInspector]
        public Vector3 ungrabVelocity;
        [HideInInspector]
        public Vector3 ungrabAngularVelocity;
        public abstract Vector3 Velocity { get; }

        public abstract Vector3 AngularVelocity { get; }

        public virtual void Grab(Grabber newGrabber)
        {
            // Find grabbable position/rotation in grabber referential
            localPositionOffset = newGrabber.transform.InverseTransformPoint(transform.position);
            localRotationOffset = Quaternion.Inverse(newGrabber.transform.rotation) * transform.rotation;
            currentGrabber = newGrabber;
        }

        public virtual void Ungrab()
        {
            currentGrabber = null;
            if (networkGrabbable)
            {
                ungrabPosition = networkGrabbable.networkTransform.InterpolationTarget.transform.position;
                ungrabRotation = networkGrabbable.networkTransform.InterpolationTarget.transform.rotation;
                ungrabVelocity = Velocity;
                ungrabAngularVelocity = AngularVelocity;
            }
        }
    }
}
