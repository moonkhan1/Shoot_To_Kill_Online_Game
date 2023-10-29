using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class HPHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnHPChanged))] 
    int _Hp { get; set; }
    
    [Networked(OnChanged = nameof(OnStateChanged))]
    bool IsDead { get; set; }
    private const int _startHp = 5;
    private bool IsInitialized;

    public bool SkipSettingHPReset;
    [Header("UIs")] 
    [SerializeField] private Color _uiOnHitColor;
    [SerializeField] private Image _uiOnHitImage;

    [Header("On Hit Body Effects ")]
    private List<FlashMeshRenderer> _flashMeshRenderers = new();
    public System.Action OnDead;

    [SerializeField] private GameObject _playerModel; 
    [SerializeField] private GameObject _deathEffectPrefab;

    private HitboxRoot _hitboxRoot;
    private NetworkPlayer _networkPlayer;
    private CharacterMovementHandler _characterMovementHandler;
    private NetworkInGameMessagesManager _networkInGameMessagesManager;

    private void Awake()
    {
        _networkPlayer = GetComponent<NetworkPlayer>();
        _characterMovementHandler = GetComponent<CharacterMovementHandler>();
        _hitboxRoot = GetComponentInChildren<HitboxRoot>();
        _networkInGameMessagesManager = GetComponent<NetworkInGameMessagesManager>();
    }

    private void Start()
    {
        if (!SkipSettingHPReset)
        {
            _Hp = _startHp;
            IsDead = false;
        }


        ResetMeshRenderers();
        //_defaultBodyColor = _bodyMeshRenderer.material.color;
        IsInitialized = true;
    }

    public void ResetMeshRenderers()
    {
        _flashMeshRenderers.Clear();

        MeshRenderer[] meshRenderers = _playerModel.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            _flashMeshRenderers.Add(new FlashMeshRenderer(meshRenderer, null));
        }

        SkinnedMeshRenderer[] skinnedMeshRenderers = _playerModel.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            _flashMeshRenderers.Add(new FlashMeshRenderer(null, skinnedMeshRenderer));
        }
    }
    private async void OnHitAsync()
    {
        foreach (FlashMeshRenderer flashMeshRenderer in _flashMeshRenderers)
        {
            flashMeshRenderer.ChangeColor(Color.red);
        }
        if (Object.HasInputAuthority)
            _uiOnHitImage.color = _uiOnHitColor;

        await UniTask.WaitForSeconds(0.2f);
        foreach (FlashMeshRenderer flashMeshRenderer in _flashMeshRenderers)
        {
            flashMeshRenderer.RestoreColor();
        }

        if (Object.HasInputAuthority && !IsDead)
            _uiOnHitImage.color = new Color(0,0,0,0);
    }

    //Only can be called by server
    public void OnTakeDamage(string killedPlayerName, int damage)
    {
        if(IsDead) return;

        if (damage > _Hp)
            damage = _Hp;
        
        _Hp -= damage;
        Debug.Log($"Damaged {_Hp}");
        if (_Hp <= 0)
        {
            OnDead?.Invoke();
            _networkInGameMessagesManager.SendInGameRpcMessages(killedPlayerName, $"Killed <b>{_networkPlayer.networkedPlayerName}</b>");
            ServerReviveCo();
            IsDead = true;
        }
    }

    private static void OnHPChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnHpChanged value {changed.Behaviour._Hp}");

        int newHP = changed.Behaviour._Hp;
        //Load old value
        changed.LoadOld();
        int oldHP = changed.Behaviour._Hp;
        
        // if Hp is decreased
        if(newHP < oldHP)
            changed.Behaviour.OnHPReduced();
    }

    
    private void OnHPReduced()
    {
        if (!IsInitialized)
        {
            return;
        }
        OnHitAsync();
    }
    private static void OnStateChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnStateChanged IsDead {changed.Behaviour.IsDead}");

        bool IsDeadCurrent = changed.Behaviour.IsDead;
        
        //Load old value
        changed.LoadOld();

        bool IsDeadOld = changed.Behaviour.IsDead;
        
        //Check if player dead. If player was dead but now should be revived
        if (IsDeadCurrent)
        {
            changed.Behaviour.OnPlayerDead();
        }
        else if (!IsDeadCurrent && IsDeadOld)
        {
            changed.Behaviour.WaitForRevive();
        }
    }

    private void OnPlayerDead()
    {
        _playerModel.gameObject.SetActive(false);
        _hitboxRoot.HitboxRootActive = false;
        _characterMovementHandler.SetCharacterControllerEnabled(false);  

        Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);
    }
    private void WaitForRevive()
    {
        if(Object.HasInputAuthority) 
            _uiOnHitImage.color = new Color(0,0,0,0);
        
        _playerModel.gameObject.SetActive(true);
        _hitboxRoot.HitboxRootActive = true;
        _characterMovementHandler.SetCharacterControllerEnabled(true);        
    }

    public void OnRespawned()
    {
        _Hp = _startHp;
        IsDead = false;
    }

    private async void ServerReviveCo()
    {
        await UniTask.WaitForSeconds(2f);
        _characterMovementHandler.RequestRespawn();
    }
}

