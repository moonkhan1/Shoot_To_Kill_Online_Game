using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using NanoSockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : SingletonBase<NetworkRunnerHandler>
{
    public NetworkRunner networkRunnerPrefab;
    private NetworkRunner networkRunner;

    private void Awake()
    {
        MakeSingleton(this);
    }

    private void Start()
    {
        if(networkRunner != null) return;
        
        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "Network Runner";

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            var clientTask = InitializeNetworkRunner(networkRunner, GameMode.AutoHostOrClient, "Test",
                GameManager.Instance.ConnectionToken, NetAddress.Any(),
                SceneManager.GetActiveScene().buildIndex, null);
        }
        Debug.Log("Server Runner started...");
    }

    public void StartHostMigration(HostMigrationToken hostMigrationToken)
    {
        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "Network Runner - Migrated";

        var clientTask = InitializeNetworkRunnerHostMigration(networkRunner, hostMigrationToken); 
        
        Debug.Log("Host Migration started...");
    }

    INetworkSceneManager GetSceneManager(NetworkRunner runner)
    {
        var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();

        if (sceneManager == null)
        {
            sceneManager = runner.AddComponent<NetworkSceneManagerDefault>();
        }

        return sceneManager;
    }
    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode,string sessionName,
        byte[] connectionToken, NetAddress address, SceneRef scene, Action<NetworkRunner> initialized)
    {

        var sceneManager = GetSceneManager(runner);
        runner.ProvideInput = true;

        return runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = sessionName,
            CustomLobbyName = "LobbyID", // All added sessions will be listed under
            Initialized = initialized,
            SceneManager = sceneManager,
            ConnectionToken =  connectionToken
        });
    }
    
    protected virtual Task InitializeNetworkRunnerHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

        var sceneManager = GetSceneManager(runner);
        runner.ProvideInput = true;

        return runner.StartGame(new StartGameArgs
        {
            SceneManager = sceneManager,
            HostMigrationToken = hostMigrationToken, // contains info to restart runner
            HostMigrationResume = HostMigrationResume, // will invoke to resume simulation
            ConnectionToken = GameManager.Instance.ConnectionToken
        });
    }

    private void HostMigrationResume(NetworkRunner runner)
    {
        Debug.Log($"{nameof(HostMigrationResume)} started");
        
        //Get for each object reference from old host
        foreach (var resumeNetworkObject in runner.GetResumeSnapshotNetworkObjects())
        {
            //Grab all the player objects' NetworkCharacterControllerPrototypeCustom
            if (resumeNetworkObject.TryGetBehaviour<NetworkCharacterControllerPrototypeCustom>(
                    out var characterController))
            {
                runner.Spawn(resumeNetworkObject, position: characterController.ReadPosition(),
                    rotation: characterController.ReadRotation(), onBeforeSpawned:
                    (runner, newNetworkObject) =>
                    {
                        newNetworkObject.CopyStateFrom(resumeNetworkObject);
                        //Copy info state from old behavior to new
                        if (resumeNetworkObject.TryGetBehaviour(out HPHandler oldHpHandler))
                        {
                            HPHandler newHpHandler = newNetworkObject.GetComponent<HPHandler>();
                            newHpHandler.CopyStateFrom(oldHpHandler);

                            newHpHandler.SkipSettingHPReset = true;
                        }
                        // Map the connection token with the new Network player

                        if (resumeNetworkObject.TryGetBehaviour<NetworkPlayer>(out var oldNetworkPlayer))
                        {
                            //Store Player token for reconnection
                            FindObjectOfType<Spawner>().SetConnectionTokenMapping(oldNetworkPlayer.Token, 
                                newNetworkObject.GetComponent<NetworkPlayer>());
                        }
                    });
            }
        }

        DeSpawnPlayerOnMigrationAsync();
        runner.SetActiveScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log($"{nameof(HostMigrationResume)} ended");
    }

    private async void DeSpawnPlayerOnMigrationAsync()
    {
        await UniTask.WaitForSeconds(3.5f);
        FindObjectOfType<Spawner>().OnHostMigrationDeSpawnLeftPlayer();
    }

    public void OnJoinLobby()
    {
        var clientTask = JoinLobby();
    }
    
    private async Task JoinLobby()
    {
        string lobbyId = "LobbyID";

        var result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyId);
        if (!result.Ok)
        {
            Debug.LogError($"Unable to join lobby with id {lobbyId}");
        }
        else
        {
            Debug.Log("Joined a lobby");
        }
        
    }

    public void CreateGame(string sessionName, string sceneName)
    {
        Debug.Log($"Create session {sessionName} scene {sceneName} build index {SceneUtility.GetBuildIndexByScenePath($"Scenes/{sceneName}")}");
        
        //Create a game as a Host
        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.Host, sessionName,
            GameManager.Instance.ConnectionToken, NetAddress.Any(),
            SceneUtility.GetBuildIndexByScenePath($"Scenes/{sceneName}"), null);
    }

    public void JoinGame(SessionInfo sessionInfo)
    {
        Debug.Log($"Join session {sessionInfo.Name}");

        //Join as a client
        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.Client, sessionInfo.Name,
            GameManager.Instance.ConnectionToken, NetAddress.Any(), SceneManager.GetActiveScene().buildIndex, null);
    }
}
