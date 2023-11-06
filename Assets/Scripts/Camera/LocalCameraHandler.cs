using Cinemachine;
using UnityEngine;

public class LocalCameraHandler : MonoBehaviour
{
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    private CinemachineVirtualCamera _cinemachineVirtualCamera;
    
    public Camera localCamera;
    [SerializeField] private Transform _cameraFollowPoint;
    [SerializeField] private GameObject _localGun;

    private Vector2 _rotationInput;

    private float _cameraRotationX;
    private float _cameraRotationY;


    private void Awake()
    {
        localCamera = GetComponent<Camera>();
        _networkCharacterControllerPrototypeCustom = GetComponentInParent<NetworkCharacterControllerPrototypeCustom>();
    }
    private void Start()
    {
        _cameraRotationX = GameManager.Instance.CameraViewRotation.x;
        _cameraRotationY = GameManager.Instance.CameraViewRotation.y;
    }

    private void LateUpdate()
    {
        if(_cameraFollowPoint == null) return; 
        
        if(!localCamera.enabled) return;

        if(_cinemachineVirtualCamera == null)
        {
            Debug.Log("_cinemachineVirtualCamera == null");
            _cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }
        else
        {
            if(NetworkPlayer.Local.IsThirdPersonCamera)
            {
                Debug.Log("IsThirdPersonCamera");

                if (_cinemachineVirtualCamera.enabled) return;

                Debug.Log("!_cinemachineVirtualCamera.enabled");

                _cinemachineVirtualCamera.Follow = NetworkPlayer.Local.model;
                _cinemachineVirtualCamera.LookAt= NetworkPlayer.Local.model;
                _cinemachineVirtualCamera.enabled = true;
                Utils.SetRenderLayerInChildren(NetworkPlayer.Local.model, LayerMask.NameToLayer("Default"));
                _localGun.SetActive(false);
            }
            else
            {
                Debug.Log("!NetworkPlayer.Local.IsThirdPersonCamera");

                if (_cinemachineVirtualCamera.enabled)
                {
                    Debug.Log("_cinemachineVirtualCamera.enabled)");

                    _cinemachineVirtualCamera.enabled = false;
                    Utils.SetRenderLayerInChildren(NetworkPlayer.Local.model, LayerMask.NameToLayer("LocalPlayerModel"));

                    _localGun.SetActive(true);
                }
            }
        }

        // Move camera to player position
        localCamera.transform.position = _cameraFollowPoint.position;
        
        // Rotate Camera
        _cameraRotationX += -_rotationInput.y * Time.deltaTime *
                           _networkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;
        _cameraRotationX = Mathf.Clamp(_cameraRotationX, -60, 60);
        
        _cameraRotationY += _rotationInput.x * Time.deltaTime * 
                           _networkCharacterControllerPrototypeCustom.rotationSpeed;
        
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
