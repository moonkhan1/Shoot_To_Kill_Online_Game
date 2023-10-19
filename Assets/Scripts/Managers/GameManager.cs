using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    private byte[] _connectionToken;

    public byte[] ConnectionToken
    {
        get => _connectionToken;
        set => _connectionToken = value;
    }
    private Vector2 _cameraViewRotation;

    public Vector2 CameraViewRotation
    {
        get => _cameraViewRotation;
        set => _cameraViewRotation = value;
    }

    private void Start()
    {
        if (_connectionToken == null)
        {
            _connectionToken = ConnectionTokenUtils.NewToken();
            Debug.Log($"Player connection token {ConnectionTokenUtils.HashToken(_connectionToken)}");
        }
    }
    private void Awake()
    {
        MakeSingleton(this);
    }
    
    
}
