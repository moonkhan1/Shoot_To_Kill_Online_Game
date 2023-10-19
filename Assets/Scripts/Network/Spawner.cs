using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkPlayer playerPrefab;
    private CharacterInputHandler _characterInputHandler;
    private SessionListHandler _sessionListHandler;

    // Mapping between Tokens and recreate players 
    private Dictionary<int, NetworkPlayer> _mapTokenIdWithNetworkPlayer;


    private void Awake()
    {
        _mapTokenIdWithNetworkPlayer = new();
        _sessionListHandler = FindObjectOfType<SessionListHandler>(true);
    }

    internal int GetPlayerToken(NetworkRunner runner, PlayerRef playerRef)
    {
        if (runner.LocalPlayer == playerRef)
        {
            //Local Player Connection token
            return ConnectionTokenUtils.HashToken(GameManager.Instance.ConnectionToken);
        }
        else
        {
            // Get connection token when Client join to this Host
            var token = runner.GetPlayerConnectionToken(playerRef);

            if (token != null)
                return ConnectionTokenUtils.HashToken(token);
 
            Debug.LogError($"{nameof(GetPlayerToken)}Returned Invalid Token");
            return 0;
        }
    }

    public void SetConnectionTokenMapping(int token, NetworkPlayer networkPlayer)
    {
        _mapTokenIdWithNetworkPlayer.Add(token, networkPlayer);
    }
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            //Get token for the player
            int playerToken = GetPlayerToken(runner, player);
            Debug.Log($"OnPlayerJoined we are server. Connection token {playerToken}");

            // Check if the token already recorded by server
            if (_mapTokenIdWithNetworkPlayer.TryGetValue(playerToken, out NetworkPlayer networkPlayer))
            {
                networkPlayer.GetComponent<NetworkObject>().AssignInputAuthority(player);
                networkPlayer.Spawned();
            }
            else
            {
                NetworkPlayer spawnedNetworkPlayer = runner.Spawn(playerPrefab, Utils.GetRandomSpawnPoint(), Quaternion.identity, player);
                
                //Store token for player
                spawnedNetworkPlayer.Token = playerToken;
                
                //Store the mapping between playerToken and spawned network player
                _mapTokenIdWithNetworkPlayer[playerToken] = spawnedNetworkPlayer;
            }
            // playerPrefab.gameObject.SetActive(true);
        }
        Debug.Log("OnPlayerJoined");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (_characterInputHandler == null && NetworkPlayer.Local != null)
        {
            _characterInputHandler = NetworkPlayer.Local.GetComponent<CharacterInputHandler>();
        }

        if (_characterInputHandler != null)
        {
            input.Set(_characterInputHandler.GetNetworkData());
        }
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("OnShutdown");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
       Debug.Log("OnConnectedToServer");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log("OnDisconnectedFromServer");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log("OnConnectRequest");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log("OnConnectFailed");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (_sessionListHandler == null) return;

        if(sessionList.Count == 0)
        {
            Debug.Log("Joined lobby, no session found");
            _sessionListHandler.OnNoSessionFound();
        }
        else
        {
            _sessionListHandler.ClearList();

            foreach (SessionInfo sessionInfo in sessionList)
            {
                _sessionListHandler.AddToList(sessionInfo);

                Debug.Log($"Found session: {sessionInfo.Name} player Count : {sessionInfo.PlayerCount}");
            }
        }
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public async void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log(nameof(OnHostMigration));

        //Shutdown current runner
        await runner.Shutdown(shutdownReason: ShutdownReason.HostMigration);

        FindObjectOfType<NetworkRunnerHandler>().StartHostMigration(hostMigrationToken);
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
    public void OnHostMigrationDeSpawnLeftPlayer()
    {
        Debug.Log($"Spawner {nameof(OnHostMigrationDeSpawnLeftPlayer)} started");

        List<int> keysToRemove = new List<int>();
        
        foreach (KeyValuePair<int, NetworkPlayer> networkTokenPlayerPair in _mapTokenIdWithNetworkPlayer)
        {
            NetworkObject networkObjectInDict = networkTokenPlayerPair.Value.GetComponent<NetworkObject>();

            // If there is a player No one has input authority over it, that mean he left
            if (networkObjectInDict.InputAuthority.IsNone)
            {
                networkObjectInDict.Runner.Despawn(networkObjectInDict);
                keysToRemove.Add(networkTokenPlayerPair.Key);
            }
        }
        // Remove the found players from the dictionary
        foreach (int key in keysToRemove)
        {
            _mapTokenIdWithNetworkPlayer.Remove(key);
        }
        Debug.Log($"Spawner {nameof(OnHostMigrationDeSpawnLeftPlayer)} ended");
    }
}
