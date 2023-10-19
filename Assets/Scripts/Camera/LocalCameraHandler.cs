using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LocalCameraHandler : MonoBehaviour
{
    private NetworkCharacterControllerPrototypeCustom _NetworkCharacterControllerPrototypeCustom;
    
    public Camera localCamera;
    [SerializeField] private Transform _cameraFollowPoint;

    private Vector2 _rotationInput;

    private float _cameraRotationX;
    private float _cameraRotationY;

    private void Start()
    {
        _cameraRotationX = GameManager.Instance.CameraViewRotation.x;
        _cameraRotationY = GameManager.Instance.CameraViewRotation.y;
    }

    private void Awake()
    {
        localCamera = GetComponent<Camera>();
        _NetworkCharacterControllerPrototypeCustom = GetComponentInParent<NetworkCharacterControllerPrototypeCustom>(); 
    }

    private void LateUpdate()
    {
        if(_cameraFollowPoint == null) return; 
        
        if(!localCamera.enabled) return;

        // Move camera to player position
        localCamera.transform.position = _cameraFollowPoint.position;
        
        // Rotate Camera
        _cameraRotationX += -_rotationInput.y * Time.deltaTime *
                           _NetworkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;
        _cameraRotationX = Mathf.Clamp(_cameraRotationX, -60, 60);
        
        _cameraRotationY += _rotationInput.x * Time.deltaTime * 
                           _NetworkCharacterControllerPrototypeCustom.rotationSpeed;
        
        //Apply rotation to camera
        localCamera.transform.rotation = Quaternion.Euler(_cameraRotationX,_cameraRotationY, 0);
    }
    
    public void SetViewInputVector(Vector2 rotationInput)
    {
        _rotationInput = rotationInput;
    }

    private void OnDestroy()
    {
        if (_cameraRotationX != 0 && _cameraRotationY != 0)
        {
            var instanceCameraViewRotation = GameManager.Instance.CameraViewRotation;
            instanceCameraViewRotation.x = _cameraRotationX;
            instanceCameraViewRotation.y = _cameraRotationY;
            GameManager.Instance.CameraViewRotation = instanceCameraViewRotation;
        }
    }
}
