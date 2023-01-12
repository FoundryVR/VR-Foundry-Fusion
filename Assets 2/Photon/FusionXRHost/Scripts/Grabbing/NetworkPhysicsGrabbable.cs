using Fusion.XR.Host.Rig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Host.Grabbing
{
    [RequireComponent(typeof(PhysicsGrabbable))]
    [OrderAfter(typeof(NetworkGrabber), typeof(NetworkHand), typeof(NetworkRigidbody))]
    public class NetworkPhysicsGrabbable : NetworkGrabbable, IBeforeTick
    {
        [HideInInspector]
        public NetworkRigidbody networkRigidbody;

        public bool IsLocalPlayerMostRecentGrabber => Runner.LocalPlayer == lastGrabbingUser;

        #region Feedback configuration         
        [System.Serializable]
        public struct PseudoHapticFeedbackConfiguration
        {
            public bool enablePseudoHapticFeedback;
            public float minNonContactingDissonance;
            public float minContactingDissonance;
            public float maxDissonanceDistance;
            public float vibrationDuration;
        }

        [Header("Feedback configuration")]
        public PseudoHapticFeedbackConfiguration pseudoHapticFeedbackConfiguration = new PseudoHapticFeedbackConfiguration {
            enablePseudoHapticFeedback = true,
            minNonContactingDissonance = 0.05f,
            minContactingDissonance = 0.005f,
            maxDissonanceDistance = 0.60f,
            vibrationDuration = 0.06f
        };
        #endregion

        [HideInInspector]
        public bool isPseudoHapticDisplayed = false;

        bool isColliding = false;
        [HideInInspector]
        public PhysicsGrabbable grabbable;
        PlayerRef lastGrabbingUser;

        Tick startGrabbingTick = -1;
        Tick endGrabbingTick = -1;
        Vector3 lastUngrabVelocity;
        Vector3 lastUngrabAngularVelocity;


        private void Awake()
        {
            networkTransform = GetComponent<NetworkTransform>();
            networkRigidbody = GetComponent<NetworkRigidbody>();
            grabbable = GetComponent<PhysicsGrabbable>();
        }

        #region NetworkGrabbable
        public override void Ungrab(NetworkGrabber grabber, GrabInfo newGrabInfo)
        {
            // If we are the player ungrabbing, and we displayed the ghost hands, we hide them
            if (IsLocalPlayerMostRecentGrabber && currentGrabber != null && currentGrabber.hand.LocalHardwareHand != null && currentGrabber.hand.LocalHardwareHand.localHandRepresentation != null)
            {
                currentGrabber.hand.LocalHardwareHand.localHandRepresentation.DisplayMesh(false);
            }

            if (currentGrabber != grabber)
            {
                // This object as been grabbed by another hand, no need to trigger an ungrab
                return;
            }

            currentGrabber = null;
            
            grabbable.rb.velocity = newGrabInfo.ungrabVelocity;
            grabbable.rb.angularVelocity = newGrabInfo.ungrabAngularVelocity;
            // We store the ungrab velocity to be able to replay it during resimulation ticks
            lastUngrabVelocity = newGrabInfo.ungrabVelocity; ;
            lastUngrabAngularVelocity = newGrabInfo.ungrabAngularVelocity;
            DidUngrab();

            // We store the precise ungrabbing tick to be able to determined if we are grabbing during resimulation tick, 
            //  where tha actual currentGrabber may have changed in the latest forward ticks
            endGrabbingTick = Runner.Tick;
        }

        public override void Grab(NetworkGrabber newGrabber, GrabInfo newGrabInfo)
        {
            grabbable.localPositionOffset = newGrabInfo.localPositionOffset;
            grabbable.localRotationOffset = newGrabInfo.localRotationOffset;

            currentGrabber = newGrabber;
            if (currentGrabber != null)
            {
                lastGrabbingUser = currentGrabber.Object.InputAuthority;
            }

            lastGrabber = currentGrabber;

            DidGrab();

            // We store the precise grabbing tick to be able to determined if we are grabbing during resimulation tick, 
            //  where tha actual currentGrabber may have changed in the latest forward ticks
            startGrabbingTick = Runner.Tick;
            endGrabbingTick = -1;
        }
        #endregion

        #region NetworkBehaviour
        public override void Spawned()
        {
            base.Spawned();
            if (Runner.Config.ServerPhysicsMode != NetworkProjectConfig.PhysicsModes.ClientPrediction)
            {
                Debug.LogError("The physics grabbing used here rely on client side physics prediction, it should be enabled for better results");
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (Runner.IsForward)
            {
                // during forward tick, the IsGrabbed is reliable as it is changed during forward ticks
                //  (more precisely, it is one tick late, due to OnChanged being called AFTER FixedUpdateNetwork,but this way every client, including proxies, can apply the same physics)
                if (IsGrabbed)
                {
                    grabbable.Follow(followedTransform: currentGrabber.transform, elapsedTime: Runner.DeltaTime);
                }
            }
            if (Runner.IsResimulation)
            {
                bool isGrabbedDuringTick = false;
                if (startGrabbingTick != -1 && Runner.Tick >= startGrabbingTick)
                {
                    if (Runner.Tick < endGrabbingTick || endGrabbingTick == -1)
                    {
                        isGrabbedDuringTick = true;
                    }
                }

                if (isGrabbedDuringTick)
                {
                    grabbable.Follow(followedTransform: lastGrabber.transform, elapsedTime: Runner.DeltaTime);
                }

                // For resim, we reapply the release velocity on the Ungrab tick, like it was done in the Forward tick where it occurred first.
                if (endGrabbingTick == Runner.Tick)
                {
                    grabbable.rb.velocity = lastUngrabVelocity;
                    grabbable.rb.angularVelocity = lastUngrabAngularVelocity;
                }
            }
        }

        public override void Render()
        {
            base.Render();

            if (IsGrabbed)
            {
                var handVisual = currentGrabber.hand.networkTransform.InterpolationTarget.transform;
                var grabbableVisual = networkTransform.InterpolationTarget.transform;

                // On remote user, we want the hand to stay glued to the object, even though the hand and the grabbed object may have various interpolation
                handVisual.rotation = grabbableVisual.rotation * Quaternion.Inverse(grabbable.localRotationOffset);
                handVisual.position = grabbableVisual.position - (handVisual.TransformPoint(grabbable.localPositionOffset) - handVisual.position);

                // Add pseudo haptic feedback if needed
                ApplyPseudoHapticFeedback();
            }
        }
        #endregion

        #region Collision handling and feedback

        void IBeforeTick.BeforeTick()
        {
            // We reset the IsColliding state before the actual tick physics simulation, where it may be set to true by OnCollisionStay
            isColliding = false;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (Object)
            {
                isColliding = true;
            }
        }

        // Display a ghost" hand at the position of the real life hand when the distance between the representation (glued to the grabbed object, and driven by forces) and the IRL hand becomes too great
        //  Also apply a vibration proportionnal to this distance, so that the user can feel the dissonance between what they ask and what they can do
        void ApplyPseudoHapticFeedback()
        {
            if (pseudoHapticFeedbackConfiguration.enablePseudoHapticFeedback && IsGrabbed && IsLocalPlayerMostRecentGrabber)
            {
                if (currentGrabber.hand.LocalHardwareHand.localHandRepresentation != null)
                {
                    var handVisual = currentGrabber.hand.networkTransform.InterpolationTarget.transform;
                    Vector3 dissonanceVector = handVisual.position - currentGrabber.hand.LocalHardwareHand.transform.position;
                    float dissonance = dissonanceVector.magnitude;
                    isPseudoHapticDisplayed = (isColliding && dissonance > pseudoHapticFeedbackConfiguration.minContactingDissonance);
                    currentGrabber.hand.LocalHardwareHand.localHandRepresentation.DisplayMesh(isPseudoHapticDisplayed);
                    if (isPseudoHapticDisplayed)
                    {
                        currentGrabber.hand.LocalHardwareHand.SendHapticImpulse(amplitude: Mathf.Clamp01(dissonance / pseudoHapticFeedbackConfiguration.maxDissonanceDistance), duration: pseudoHapticFeedbackConfiguration.vibrationDuration);
                    }
                }
            }
        }
        #endregion
    }
}
