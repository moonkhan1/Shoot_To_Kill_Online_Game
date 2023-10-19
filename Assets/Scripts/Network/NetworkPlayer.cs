using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    [SerializeField] private TextMeshProUGUI _playerName;
    
    [Networked(OnChanged = nameof(OnNameChanged))]
    public NetworkString<_16> networkedPlayerName { get; private set; }
    
    // Remote Client Token Hash
    [Networked] public int Token { get; set; }
    public static NetworkPlayer Local { get; private set; }
    [SerializeField] private Transform model;
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    private bool _isPlayerJoinedGame = false;
    private NetworkInGameMessagesManager _networkInGameMessagesManager;
    
    public LocalCameraHandler localCameraHandler;
    [SerializeField] private GameObject _localUI;
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
        if (Object.HasInputAuthority)
        {
            Local = this;
            
            //Set layer of local Player's model children
            Utils.SetRenderLayerInChildren(model, LayerMask.NameToLayer("LocalPlayerModel"));
            
            //Disable main Camera on player spawned
            if(Camera.main != null)
                Camera.main.gameObject.SetActive(false);

            AudioListener playerAudioListener = GetComponentInChildren<AudioListener>(true); //Enable disabled listeners
            playerAudioListener.enabled = true;
            
            localCameraHandler.localCamera.enabled = true;

            //Detach camera for player
            localCameraHandler.transform.parent = null;
            
            _localUI.SetActive(true);
            
            RPC_SetName(PlayerPrefs.GetString("PlayerName"));
            Debug.Log("Spawned local player");
        }
        else
        {
            //Disable player camera if left
            localCameraHandler.localCamera.enabled = false;
            
            _localUI.SetActive(false);
            
            //Only 1 audioListener allowed. Disable the players'
            AudioListener playerAudioListener = GetComponentInChildren<AudioListener>();
            playerAudioListener.enabled = false;
            
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

    private void OnDestroy()
    {
        //Destroy local camera if local player destroyed because new one will be spawned with own local camera 
        if(localCameraHandler != null)
            Destroy(localCameraHandler.gameObject);
    }
}
