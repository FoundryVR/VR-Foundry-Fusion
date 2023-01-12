using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Fusion.XR.Host.Grabbing
{
    /**
     * This optional components display a ghost of a graqbbed object at the position it would be if it was not colliding, thus following the ghost hand
     */
    [RequireComponent(typeof(NetworkPhysicsGrabbable))]
    [OrderAfter(typeof(NetworkPhysicsGrabbable))]
    public class PseudoHapticGrabbableRender : MonoBehaviour
    {
        public Material ghostMetarial;

        NetworkPhysicsGrabbable networkGrabbable;
        bool isPseudoHapticDisplayed = false;

        GameObject ghostObject;
        Renderer[] ghostRenderers;

        void Awake()
        {
            networkGrabbable = GetComponent<NetworkPhysicsGrabbable>();
        }

        void CreateGhost()
        {
            ghostObject = GameObject.Instantiate(networkGrabbable.networkTransform.InterpolationTarget.gameObject);
            ghostObject.transform.localScale = networkGrabbable.networkTransform.InterpolationTarget.transform.lossyScale;
            ghostObject.transform.parent = networkGrabbable.networkTransform.InterpolationTarget.transform;
            ghostRenderers = ghostObject.GetComponentsInChildren<Renderer>();
            if(ghostMetarial == null)
            {
                if (networkGrabbable.currentGrabber && networkGrabbable.currentGrabber.hand.LocalHardwareHand && networkGrabbable.currentGrabber.hand.LocalHardwareHand.localHandRepresentation != null)
                {
                    ghostMetarial = networkGrabbable.currentGrabber.hand.LocalHardwareHand.localHandRepresentation.SharedHandMaterial;
                }
            }
            if (ghostMetarial)
            {
                var material = networkGrabbable.currentGrabber.hand.LocalHardwareHand.localHandRepresentation.SharedHandMaterial;
                foreach(var renderer in ghostRenderers)
                {
                    renderer.sharedMaterial = ghostMetarial = networkGrabbable.currentGrabber.hand.LocalHardwareHand.localHandRepresentation.SharedHandMaterial;
                }
            }
        }

        void SetGhostVisibility(bool visible)
        {
            foreach (var renderer in ghostRenderers)
            {
                renderer.enabled = visible;
            }
            ghostObject.SetActive(visible);
        }

        private void LateUpdate()
        {
            if (networkGrabbable.isPseudoHapticDisplayed && networkGrabbable.currentGrabber && networkGrabbable.currentGrabber.hand.LocalHardwareHand)
            {
                if (!isPseudoHapticDisplayed)
                {
                    // Display ghost object
                    if (!ghostObject) CreateGhost();
                    SetGhostVisibility(true);
                    isPseudoHapticDisplayed = true;
                }

                // Move ghost object: follow ghost hand
                Transform ghostHand = networkGrabbable.currentGrabber.hand.LocalHardwareHand.transform;
                ghostObject.transform.position = ghostHand.TransformPoint(networkGrabbable.grabbable.localPositionOffset);
                ghostObject.transform.rotation = ghostHand.rotation * networkGrabbable.grabbable.localRotationOffset;
            } else
            {
                if (isPseudoHapticDisplayed)
                {
                    // Hide ghost object
                    SetGhostVisibility(false);
                    isPseudoHapticDisplayed = false;
                }
            }
        }
    }

}
