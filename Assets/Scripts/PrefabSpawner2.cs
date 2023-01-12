#if FUSION_WEAVER
using Fusion;
using Fusion.Sockets;

namespace Photon.Voice.Fusion.Demo
{
    using UnityEngine;
    using System.Collections.Generic;
    using System;

    public class PrefabSpawner2 : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField]
        private NetworkObject prefab;

        private Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();

        [SerializeField]
        private bool debugLogs;


        private bool fistPlayer;
        [SerializeField] private Transform startTrans1, startTrans2;
       // [SerializeField] private Vector3 startPos1, startPos2;
       // [SerializeField] private Quaternion startRotation1, startRotation2;
        
        

#region INetworkRunnerCallbacks

        void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (this.debugLogs)
            {
                Debug.Log($"OnPlayerJoined {player} mode = {runner.GameMode}");
            }
            switch (runner.GameMode)
            {
                case GameMode.Single:
                case GameMode.Server:
                case GameMode.Host:
                    this.SpawnPlayer(runner, player);
                    break;
            }
        }

        void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (this.debugLogs)
            {
                Debug.Log($"OnPlayerLeft {player} mode = {runner.GameMode}");
            }
            switch (runner.GameMode)
            {
                case GameMode.Single:
                case GameMode.Server:
                case GameMode.Host:
                    this.TryDespawnPlayer(runner, player);
                    break;
            }
        }

        void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (this.debugLogs)
            {
                Debug.Log($"OnShutdown mode = {runner.GameMode} reason = {shutdownReason}");
                foreach (var pair in this.spawnedPlayers)
                {
                    Debug.LogWarning($"Prefab not despawned? {pair.Key}:{pair.Value?.Id}");
                }
            }
        }

        void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
        {
            if (this.debugLogs)
            {
                Debug.Log($"OnConnectedToServer mode = {runner.GameMode}");
            }
            if (runner.GameMode == GameMode.Shared)
            {
                this.SpawnPlayer(runner, runner.LocalPlayer);
            }
        }

        void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner)
        {
            if (this.debugLogs)
            {
                Debug.Log($"OnDisconnectedFromServer mode = {runner.GameMode}");
            }
            if (runner.GameMode == GameMode.Shared)
            {
                this.TryDespawnPlayer(runner, runner.LocalPlayer);
            }
        }

        void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
        }

        void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
        }

        void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
        {
        }

        void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
        {
        }

#endregion

        private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
        {
            
            
            //NetworkObject instance = runner.Spawn(this.prefab, transform.position,transform.rotation, player);
            
            
            NetworkObject instance;
            if (fistPlayer == false)
            {
                 instance = runner.Spawn(this.prefab, startTrans1.position,startTrans1.rotation, player);
                fistPlayer = true;
            }
            else
            {
                 instance = runner.Spawn(this.prefab, startTrans2.position,startTrans2.rotation, player);
            }

            if (this.debugLogs)
            {
                if (this.spawnedPlayers.TryGetValue(player, out NetworkObject oldValue))
                {
                    Debug.LogWarning($"Replacing NO {oldValue?.Id} w/ {instance?.Id} for {player}");
                }
                else
                {
                    Debug.Log($"Spawned NO {instance?.Id} for {player}");
                }
            }
            this.spawnedPlayers[player] = instance;
        }

        private bool TryDespawnPlayer(NetworkRunner runner, PlayerRef player)
        {
            if (this.spawnedPlayers.TryGetValue(player, out NetworkObject instance))
            {
                if (this.debugLogs)
                {
                    Debug.Log($"Despawning NO {instance?.Id} for {player}");
                }
                runner.Despawn(instance);
                return this.spawnedPlayers.Remove(player);
            }
            if (this.debugLogs)
            {
                Debug.LogWarning($"No spawned NO found for player {player}");
            }
            return false;
        }
    }
}
#endif
