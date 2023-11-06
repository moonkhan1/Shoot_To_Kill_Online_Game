using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RevolverController : MonoBehaviour
{
    private Animator _anim;
    private PlayerInputCustom _playerInputCustom;


    private void Awake()
    {
        _playerInputCustom = new PlayerInputCustom();
        _anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _playerInputCustom.Enable();
        _playerInputCustom.Player.Fire.started += OnFire;
        _playerInputCustom.Player.Fire.performed += OnFire;
        _playerInputCustom.Player.Fire.canceled += OnFire;
    }

    private void OnDisable()
    {
        _playerInputCustom.Player.Fire.started -= OnFire;
        _playerInputCustom.Player.Fire.performed -= OnFire;
        _playerInputCustom.Player.Fire.canceled -= OnFire;
        _playerInputCustom.Disable();
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        if (context.action.WasPressedThisFrame())
        {
            _anim.Play("Shoot");
            _anim.Play("Shoot2");

            
        }
    }
}
