using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInputHandler : MonoBehaviour
{
    public Vector3 Direction;
    public Vector2 Rotation;
    public bool IsJumping;
    public bool IsFiring;
    public bool IsThrown;
    private PlayerInputCustom _playerInputCustom;
    private LocalCameraHandler _localCameraHandler;
    private void Awake()
    {
        _playerInputCustom = new PlayerInputCustom();
        _localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        _playerInputCustom.Enable();
        _playerInputCustom.Player.Movement.started += OnMove;
        _playerInputCustom.Player.Movement.performed += OnMove;
        _playerInputCustom.Player.Movement.canceled += OnMove;
        
        _playerInputCustom.Player.Rotator.started += OnRotate;
        _playerInputCustom.Player.Rotator.performed += OnRotate;
        _playerInputCustom.Player.Rotator.canceled += OnRotate;
        
        _playerInputCustom.Player.Jump.started += OnJump;
        _playerInputCustom.Player.Jump.performed += OnJump;
        _playerInputCustom.Player.Jump.canceled += OnJump;
        
        _playerInputCustom.Player.Fire.started += OnFire;
        _playerInputCustom.Player.Fire.performed += OnFire;
        _playerInputCustom.Player.Fire.canceled += OnFire;
        
        _playerInputCustom.Player.Throw.started += OnThrow;
        _playerInputCustom.Player.Throw.performed += OnThrow;
        _playerInputCustom.Player.Throw.canceled += OnThrow;
    }
    
    private void OnDisable()
    {
        _playerInputCustom.Player.Movement.started -= OnMove;
        _playerInputCustom.Player.Movement.performed -= OnMove;
        _playerInputCustom.Player.Movement.canceled -= OnMove;
        
        _playerInputCustom.Player.Rotator.started -= OnRotate;
        _playerInputCustom.Player.Rotator.performed -= OnRotate;
        _playerInputCustom.Player.Rotator.canceled -= OnRotate;
        
        _playerInputCustom.Player.Jump.started -= OnJump;
        _playerInputCustom.Player.Jump.performed -= OnJump;
        _playerInputCustom.Player.Jump.canceled -= OnJump;
        
        _playerInputCustom.Player.Fire.started -= OnFire;
        _playerInputCustom.Player.Fire.performed -= OnFire;
        _playerInputCustom.Player.Fire.canceled -= OnFire;
        
        _playerInputCustom.Player.Throw.started -= OnThrow;
        _playerInputCustom.Player.Throw.performed -= OnThrow;
        _playerInputCustom.Player.Throw.canceled -= OnThrow;
    
        _playerInputCustom.Disable();
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 oldDirection = context.ReadValue<Vector2>();
        Direction = new Vector3(oldDirection.x, 0f, oldDirection.y);
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        Rotation = context.ReadValue<Vector2>();
        _localCameraHandler.SetViewInputVector(Rotation);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        IsJumping = context.action.WasPressedThisFrame();
    }
    
    public void OnFire(InputAction.CallbackContext context)
    {
        IsFiring = context.action.WasPressedThisFrame();
    }
    
    public void OnThrow(InputAction.CallbackContext context)
    {
        IsThrown = context.action.WasPressedThisFrame();
    }

    public NetworkInputData GetNetworkData()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        networkInputData.MovementInput = Direction;
        networkInputData.AimForwardVector = _localCameraHandler.transform.forward;
        networkInputData.IsFiring = IsFiring;
        networkInputData.IsJumping = IsJumping;
        networkInputData.IsThrown = IsThrown;
        IsFiring = false;
        IsJumping = false;
        IsThrown = false;
        return networkInputData;
    }
    
    
}
