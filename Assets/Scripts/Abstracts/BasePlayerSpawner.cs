using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;

public abstract class BasePlayerSpawner : NetworkBehaviour
{
    [SerializeField] protected NetworkPlayer _playerPrefab;
    public abstract void Spawn();
    public abstract void Despawn(NetworkPlayer networkPlayer);
}
