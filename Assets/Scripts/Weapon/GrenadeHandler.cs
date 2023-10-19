using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

public class GrenadeHandler : NetworkBehaviour
{
    [SerializeField] private GameObject explosionGameObject;
    private PlayerRef _thrownByPlayerRef;
    private string _thrownByName; 
    private NetworkObject _networkObject;
    private NetworkRigidbody _networkRigidbody;
    private TickTimer _explodeTickTimer = TickTimer.None;
    private List<LagCompensatedHit> _hits = new ();

    public LayerMask explosionLayer;
    
    public void Throw(Vector3 throwForce, PlayerRef thrownByPlayer, string thrownPlayerName)
    {
        _networkObject = GetComponent<NetworkObject>();
        _networkRigidbody = GetComponent<NetworkRigidbody>();
        
        _networkRigidbody.Rigidbody.AddForce(throwForce, ForceMode.Impulse);

        _thrownByPlayerRef = thrownByPlayer;
        _thrownByName = thrownPlayerName;

        _explodeTickTimer = TickTimer.CreateFromSeconds(Runner, 2);
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (_explodeTickTimer.Expired(Runner))
            {
                int hitCount =
                    Runner.LagCompensation.OverlapSphere(transform.position, 4, _thrownByPlayerRef, _hits, explosionLayer);
                for (int i = 0; i < hitCount; i++)
                {
                    HPHandler hpHandler = _hits[i].Hitbox.transform.root.GetComponent<HPHandler>();
                    if(hpHandler != null)
                        hpHandler.OnTakeDamage(_thrownByName, 20);
                }
                
                Runner.Despawn(_networkObject);

                _explodeTickTimer = TickTimer.None;
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        Transform grenadeObjectTransform = transform.GetChild(0).transform;

        Instantiate(explosionGameObject, grenadeObjectTransform.position, Quaternion.identity);
        
        
    }
}
