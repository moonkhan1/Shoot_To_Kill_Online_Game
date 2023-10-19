using System;
using System.Threading.Tasks;
using Cinemachine;
using Fusion;
using UnityEngine;
using Zenject;

public class CharacterMovementHandler : NetworkBehaviour
{
    bool isRespawnRequested = false;
    private NetworkCharacterControllerPrototypeCustom _NetworkCharacterControllerPrototypeCustom;
    private Camera localCamera;
    private HPHandler _hpHandler;

    private NetworkInGameMessagesManager _networkInGameMessagesManager;

    private NetworkPlayer _networkPlayer;
    // [Inject] private PlayerSpawner _playerSpawner;
    private void Awake()
    {
        _networkInGameMessagesManager = GetComponent<NetworkInGameMessagesManager>();
        _NetworkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>(); 
        localCamera = GetComponentInChildren<Camera>();
        _hpHandler = GetComponent<HPHandler>();
        _networkPlayer = GetComponent<NetworkPlayer>();
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (isRespawnRequested)
            {
                Respawn();
                return;
            }
            _hpHandler.OnDead += () =>
            {
                
                return;
            };
        }
        //Get inputs from network
        if (GetInput(out NetworkInputData networkInputData))
        {
            // Rotate the transform as the client aim vector
            transform.forward = networkInputData.AimForwardVector;
            //Cancel out the x axis rotation to prevent tilting
            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;
            
            //Movement
            Vector3 moveDirection = (networkInputData.MovementInput) * Runner.DeltaTime * _NetworkCharacterControllerPrototypeCustom.maxSpeed;
            _NetworkCharacterControllerPrototypeCustom.Move(moveDirection);
            
            //Jump
            if(networkInputData.IsJumping)
                _NetworkCharacterControllerPrototypeCustom.Jump();

            CheckFallRespawn();
        }
    }
    
    private void CheckFallRespawn()
    {
        if (transform.position.y < -12)
        {
            if (Object.HasStateAuthority)
            {
                _networkInGameMessagesManager.SendInGameRpcMessages(_networkPlayer.networkedPlayerName.ToString(), " Fell off the world!");
                Respawn();
            }
        }
    }

    public void RequestRespawn()
    {
        isRespawnRequested = true;
    }
    private void Respawn()
    {
        _NetworkCharacterControllerPrototypeCustom.TeleportToPosition(Utils.GetRandomSpawnPoint());
        _hpHandler.OnRespawned();
        isRespawnRequested = false;
    }
    public void SetCharacterControllerEnabled(bool isEnabled)
    {
        _NetworkCharacterControllerPrototypeCustom.Controller.enabled = isEnabled;
    }
}
