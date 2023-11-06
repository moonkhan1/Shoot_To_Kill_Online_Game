using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Zenject;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] public Transform model;
    
    [Networked(OnChanged = nameof(OnNameChanged))]
    public NetworkString<_16> networkedPlayerName { get; private set; }
    
    // Remote Client Token Hash
    [Networked] public int Token { get; set; }
    public static NetworkPlayer Local { get; private set; }
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    private bool _isPlayerJoinedGame = false;
    private NetworkInGameMessagesManager _networkInGameMessagesManager;
    
    public LocalCameraHandler localCameraHandler;
    [SerializeField] private GameObject _localUI;

    // CAMERA
    public bool IsThirdPersonCamera { get; set; }
    private void OnValidate()
    {
        GetReference();
    }

    private void Awake()
    {
        _networkInGameMessagesManager = GetComponent<NetworkInGameMessagesManager>();
    }

    public override void Spawned()
    {
        bool isInReadyScene = SceneManager.GetActiveScene().name == "ReadyScene";

        if (Object.HasInputAuthority)
        {
            Local = this;

            if (isInReadyScene)
            {
                Camera.main.transform.position = new Vector3(transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);

                localCameraHandler.gameObject.SetActive(false);
                _localUI.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                //Set layer of local Player's model children
                Utils.SetRenderLayerInChildren(model, LayerMask.NameToLayer("LocalPlayerModel"));

                //Disable main Camera on player spawned
                if (Camera.main != null)
                    Camera.main.gameObject.SetActive(false);

                //AudioListener playerAudioListener = GetComponentInChildren<AudioListener>(true); //Enable disabled listeners
                //playerAudioListener.enabled = true;

                localCameraHandler.localCamera.enabled = true;
                localCameraHandler.gameObject.SetActive(true);

                //Detach camera for player
                localCameraHandler.transform.parent = null;

                _localUI.SetActive(true);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            
            RPC_SetName(PlayerPrefs.GetString("PlayerName"));
            _playerName.gameObject.SetActive(false);
            Debug.Log("Spawned local player");
        }
        else
        {
            //Disable player camera if left
            localCameraHandler.localCamera.enabled = false;
            localCameraHandler.gameObject.SetActive(false);


            _localUI.SetActive(false);
            
            //Only 1 audioListener allowed. Disable the players'
            //AudioListener playerAudioListener = GetComponentInChildren<AudioListener>();
            //playerAudioListener.enabled = false;
            
            //Disable UI for remote player
            _localUI.SetActive(false);
            Debug.Log("Spawned remote player");
        }
        //Set the player as player object
        Runner.SetPlayerObject(Object.InputAuthority, Object);
        //Easier to separate players
        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Object.HasStateAuthority)
        {
            if (Runner.TryGetPlayerObject(player, out NetworkObject playerNetworkObject))
            {
                var networkInGameMessage = Local.GetComponent<NetworkInGameMessagesManager>();
                var networkPlayerName = playerNetworkObject.GetComponent<NetworkPlayer>().networkedPlayerName;
                
                if(playerNetworkObject != Object) return;
                networkInGameMessage.SendInGameRpcMessages(networkPlayerName.ToString(), " Left game!");
                                
            }
            
        }
        
        if (player == Object.InputAuthority)
        {
            Runner.Despawn(Object);
        }
    }
    
    private void GetReference()
    {
        if(_networkCharacterControllerPrototypeCustom == null)
            _networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
    }

    private static void OnNameChanged(Changed<NetworkPlayer> changed)
    {
        changed.Behaviour.NameSetUp();
    }
    private void NameSetUp()
    {
        _playerName.text = networkedPlayerName.ToString();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetName(string playerName, RpcInfo info = default)
    {
        networkedPlayerName = playerName;
        if (!_isPlayerJoinedGame)
        {
            _networkInGameMessagesManager.SendInGameRpcMessages(playerName, "Joined game!");
            _isPlayerJoinedGame = true;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_CameraChange(bool isThirdPersonCamera, RpcInfo info = default)
    {
        Debug.Log("Rpc_CameraChange");
        IsThirdPersonCamera = isThirdPersonCamera;
    }

    private void OnDestroy()
    {
        //Destroy local camera if local player destroyed because new one will be spawned with own local camera 
        if (localCameraHandler != null)
            Destroy(localCameraHandler.gameObject);

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name != "ReadyScene")
        {
            if(Object.HasStateAuthority && Object.HasInputAuthority)
            {
                Spawned();
            }
            if(Object.HasStateAuthority)
            {
                GetComponent<CharacterMovementHandler>().RequestRespawn();
            }
        }
    }
}
