using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Host.Grabbing
{
    public class PhysicsGrabbable : Grabbable
    {
        public Rigidbody rb;
        protected virtual void Awake()
        {
            networkGrabbable = GetComponent<NetworkGrabbable>();
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
        }
        public override Vector3 Velocity => rb.velocity;
        public override Vector3 AngularVelocity => rb.angularVelocity;
        public bool IsGrabbed => currentGrabber != null;

        #region Follow configuration        
        [Header("Follow configuration")]
        [Range(0, 1)]
        public float followVelocityAttenuation = 0.5f;
        public float maxVelocity = 10f;
        #endregion

        #region Following logic
        public virtual void Follow(Transform followedTransform, float elapsedTime)
        {
            // Compute the requested velocity to joined target position during a Runner.DeltaTime
            rb.VelocityFollow(target: followedTransform, localPositionOffset, localRotationOffset, elapsedTime);
            // To avoid a too aggressive move, we attenuate and limit a bit the expected velocity
            rb.velocity *= followVelocityAttenuation; // followVelocityAttenuation = 0.5F by default
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity); // maxVelocity = 10f by default
        }
        #endregion

        private void FixedUpdate()
        {
            // We handle the following if we are not online (in that case, the Follow will be called by the NetworkGrabbable during FUN and Render)
            if (networkGrabbable == null || networkGrabbable.Object == null)
            {
                // Note that this offline following will not offer the pseudo-haptic feedback, which relies on the NetworkTransform interpolation target (it could easily be recreated offline if needed)
                if (IsGrabbed) Follow(followedTransform: currentGrabber.transform, Time.fixedDeltaTime);
            }
        }
    }
}
