using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterModelChangeHandler : NetworkBehaviour
{
    [SerializeField] private GameObject _playerModel;
    [SerializeField] private Image _isReadyImage;

    private List<GameObject> _playerModelTypes = new List<GameObject>();


    struct NetworkPlayerModel : INetworkStruct
    {
        public byte playerModelID;
    }

    [Networked(OnChanged = nameof(OnPlayerModelChanged))]
    NetworkPlayerModel networkPlayerModel { get; set; }

    [Networked(OnChanged = nameof(OnIsChareacterModelSelectionDone))]
    public NetworkBool isDoneWithModelSelection { get; private set; }

    private void Awake()
    {
        _playerModelTypes = Resources.LoadAll<GameObject>("PlayerTypes/").ToList();
        _playerModelTypes = _playerModelTypes.OrderBy(x => x.name).ToList();
    }
    private void Start()
    {
        _isReadyImage.gameObject.SetActive(false);

        if (SceneManager.GetActiveScene().name != "ReadyScene") return;

        NetworkPlayerModel newNetworkPlayerModel = networkPlayerModel;

        newNetworkPlayerModel.playerModelID = (byte)Random.Range(0, _playerModelTypes.Count);

        //If has input authority request host to integrate new model among server
        if(Object.HasInputAuthority) RPC_RequestModelChange(newNetworkPlayerModel);
    }
    private GameObject ReplacePlayerModel(GameObject currentModel, GameObject newPlayerType)
    {
        GameObject newModel = Instantiate(newPlayerType, currentModel.transform.position, currentModel.transform.rotation);
        newModel.transform.parent = currentModel.transform.parent;
        Utils.SetRenderLayerInChildren(newModel.transform, currentModel.layer);
        GetComponent<NetworkMecanimAnimator>().Animator = newModel.GetComponent<Animator>();
        GetComponent<CharacterMovementHandler>().Anim = newModel.GetComponent<Animator>();

        Destroy(currentModel);
        
        return newModel;
    }

    internal void ReplaceModel()
    {
        _playerModel = ReplacePlayerModel(_playerModel, _playerModelTypes[networkPlayerModel.playerModelID]);
        
        GetComponent<HPHandler>().ResetMeshRenderers();
    
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestModelChange(NetworkPlayerModel newNetworkPlayerModel, RpcInfo info = default)
    {
        networkPlayerModel = newNetworkPlayerModel; 
    }

    internal static void OnPlayerModelChanged(Changed<CharacterModelChangeHandler> changed)
    {
        changed.Behaviour.OnPlayerModelChanged();
    }

    private void OnPlayerModelChanged()
    {
        ReplaceModel();
    }

    public void ModelChangeCycle()
    {
        NetworkPlayerModel newModel = networkPlayerModel;

        newModel.playerModelID++;

        if(newModel.playerModelID > _playerModelTypes.Count - 1)
        {
            newModel.playerModelID = 0;
        }
        if (Object.HasInputAuthority) RPC_RequestModelChange(newModel);
    }

    public void OnReady(bool isReady)
    {
        if(Object.HasInputAuthority)
        {
            Rpc_SetReady(isReady);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void Rpc_SetReady(NetworkBool isReady, RpcInfo info = default)
    {
        isDoneWithModelSelection = isReady;
    }

    private static void OnIsChareacterModelSelectionDone(Changed<CharacterModelChangeHandler> changed)
    {
        changed.Behaviour.IsChareacterModelSelectionDone();
    }

    private void IsChareacterModelSelectionDone()
    {
        if(isDoneWithModelSelection)
        {
            _isReadyImage.gameObject.SetActive(true);
        }
        else
        {
            _isReadyImage.gameObject.SetActive(false);
        }
    }
}
