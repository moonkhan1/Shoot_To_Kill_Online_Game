using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerSpawner : BasePlayerSpawner
{
    public static PlayerSpawner Instance { get; private set; }
    [SerializeField] private int poolSize = 1;
    private Queue<NetworkPlayer> _pool1;
    
    private void Awake()
    {
        _pool1 = new Queue<NetworkPlayer>();
        if (Instance == null)
            Instance = this;
    }
    private void Start()
    {
        GrowPool();
    }
    private void GrowPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var player = Instantiate(_playerPrefab, transform);
            _playerPrefab.gameObject.SetActive(false);
            _pool1.Enqueue(player);
        }
    }
    public override void Spawn()
    {
        if (_pool1.Count <= 0)
        {
            GrowPool();
        }
        var playerPoolObject = _pool1.Dequeue();
        playerPoolObject.transform.GetComponent<NetworkCharacterControllerPrototypeCustom>().TeleportToPosition(Utils.GetRandomSpawnPoint());
        playerPoolObject.gameObject.SetActive(true);
        Debug.Log("Player Spawned");
    }

    public override void Despawn(NetworkPlayer networkPlayer)
    {
        networkPlayer.gameObject.SetActive(false);
        _pool1.Enqueue(networkPlayer);

    }
}
