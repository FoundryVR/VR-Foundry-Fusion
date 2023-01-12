using UnityEngine;

namespace Fusion.XR.Host.Rig
{
    public interface IHandRepresentation
    {
        public void SetHandCommand(HandCommand command);
        public GameObject gameObject { get; }
        public void SetHandColor(Color color);
        public void SetHandMaterial(Material material);
        public void DisplayMesh(bool shouldDisplay);
        public bool IsMeshDisplayed { get; }
        public Material SharedHandMaterial { get; }

    }


    /**
     * 
     * Network VR user hand
     * 
     * Handle the synchronisation of the hand pose
     * Use the local HardwareRig rig hand pose when this rig is associated with the local user 
     * 
     * Position synchronization is handled in the NetworkRig
     * 
     **/

    [RequireComponent(typeof(NetworkTransform))]
    [OrderAfter(typeof(NetworkRig))]
    public class NetworkHand : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkTransform networkTransform;
        [Networked(OnChanged = nameof(OnHandCommandChange))]
        public HandCommand HandCommand { get; set; }
        public RigPart side;
        NetworkRig rig;
        IHandRepresentation handRepresentation;

        public bool IsLocalNetworkRig => rig.IsLocalNetworkRig;

        public HardwareHand LocalHardwareHand => IsLocalNetworkRig ? (side == RigPart.LeftController ? rig.hardwareRig.leftHand : rig.hardwareRig.rightHand) : null;

        private void Awake()
        {
            rig = GetComponentInParent<NetworkRig>();
            networkTransform = GetComponent<NetworkTransform>();
            handRepresentation = GetComponentInChildren<IHandRepresentation>();
        }

        public static void OnHandCommandChange(Changed<NetworkHand> changed)
        {
            // Will be called on all clients when the local user change the hand pose structure
            // We trigger here the actual animation update
            changed.Behaviour.UpdateHandRepresentationWithNetworkState();
        }

      
        public override void Render()
        {
            base.Render();
            if (IsLocalNetworkRig)
            {
                // Extrapolate for local user : we want to have the visual at the good position as soon as possible, so we force the visuals to follow the most fresh hand pose
                UpdateRepresentationWithLocalHardwareState();
            }
        }

        // Update the hand representation each time the network structure HandCommand is updated
        void UpdateHandRepresentationWithNetworkState()
        {
            if (handRepresentation != null) handRepresentation.SetHandCommand(HandCommand);
        }

        // Update the hand representation each time the network structure HandCommand is updated
        void UpdateRepresentationWithLocalHardwareState()
        {
            if (handRepresentation != null) handRepresentation.SetHandCommand(LocalHardwareHand.handCommand);
        }
    }
}

