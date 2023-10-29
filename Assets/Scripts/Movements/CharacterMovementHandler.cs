using System;
using System.Threading.Tasks;
using Cinemachine;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class CharacterMovementHandler : NetworkBehaviour
{
    [SerializeField] public Animator Anim { get; set; }
    private float _walkSpeed = 0f;

    bool isRespawnRequested = false;
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    private Camera localCamera;
    private HPHandler _hpHandler;

    private NetworkInGameMessagesManager _networkInGameMessagesManager;

    private NetworkPlayer _networkPlayer;
    // [Inject] private PlayerSpawner _playerSpawner;
    private void Awake()
    {
        _networkInGameMessagesManager = GetComponent<NetworkInGameMessagesManager>();
        _networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>(); 
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
            if (SceneManager.GetActiveScene().name == "ReadyScene") return;

            // Rotate the transform as the client aim vector
            transform.forward = networkInputData.AimForwardVector;
            //Cancel out the x axis rotation to prevent tilting
            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;
            
            //Movement
            Vector3 moveDirection = (networkInputData.MovementInput) * Runner.DeltaTime * _networkCharacterControllerPrototypeCustom.maxSpeed;
            _networkCharacterControllerPrototypeCustom.Move(moveDirection);
            
            //Jump
            if(networkInputData.IsJumping)
                _networkCharacterControllerPrototypeCustom.Jump();

            //ANIMATIONS
            if (Anim == null) return;

            //Walk Anim
            Vector2 walkDirection = new Vector2(_networkCharacterControllerPrototypeCustom.Velocity.x, _networkCharacterControllerPrototypeCustom.Velocity.z);
            walkDirection.Normalize();

            _walkSpeed = Mathf.Lerp(_walkSpeed, Mathf.Clamp01(walkDirection.magnitude), Runner.DeltaTime);

            Anim.SetFloat("Walking", _walkSpeed);

            //Attack Anim
            if(networkInputData.IsFiring)
                Anim.SetTrigger("AttackTrig");

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
        _networkCharacterControllerPrototypeCustom.TeleportToPosition(Utils.GetRandomSpawnPoint());
        _hpHandler.OnRespawned();
        isRespawnRequested = false;
    }
    public void SetCharacterControllerEnabled(bool isEnabled)
    {
        _networkCharacterControllerPrototypeCustom.Controller.enabled = isEnabled;
    }
}
