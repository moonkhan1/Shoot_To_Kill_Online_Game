using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    public ParticleSystem[] _fireParticleEffect;
    private float _lastTimeFired = 0;
    private HPHandler _hpHandler;
    private NetworkPlayer _networkPlayer;

    [SerializeField] private Transform _aimPoint;
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private GrenadeHandler grenadeHandler;
    
    [Networked(OnChanged = nameof(OnFireChanged))]
    private bool IsFiring { get; set; }
    
    TickTimer grenadeThrowDelay = TickTimer.None;
    private void Awake()
    {
        _hpHandler = GetComponent<HPHandler>();
        _networkPlayer = GetComponent<NetworkPlayer>();
    }

    public override void FixedUpdateNetwork()
    {
        _hpHandler.OnDead += () =>
        {
            return;
        };
        if (GetInput(out NetworkInputData networkInputData))
        {
            if (networkInputData.IsFiring)
                Fire(networkInputData.AimForwardVector);
            if(networkInputData.IsThrown)
                FireGrenade(networkInputData.AimForwardVector);
        }
    }

    private async void Fire(Vector3 aimForwardVector)
    {
        if(Time.time - _lastTimeFired < 0.3f) return;

        IsFiring = true;
        foreach (ParticleSystem effects in _fireParticleEffect)
        {
            effects.Play();
        }
        await UniTask.WaitForSeconds(0.09f);
        IsFiring = false;

        Runner.LagCompensation.Raycast(_aimPoint.position, aimForwardVector, 100, 
            Object.InputAuthority, out var hit, collisionLayer, HitOptions.IgnoreInputAuthority);

        float hitDistance = 100;
        bool isHitPlayer = false;

        if (hit.Distance > 0) hitDistance = hit.Distance;
        
        if(hit.Hitbox != null)
        {
            if (Object.HasStateAuthority) hit.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamage(_networkPlayer.networkedPlayerName.ToString(), 1);
            
            isHitPlayer = true;
        }

        else if (hit.Collider != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit PhysX collider {hit.Collider.transform.name}");
        }

        //DEBUG
        Debug.DrawRay(_aimPoint.position, aimForwardVector * hitDistance, isHitPlayer ? Color.red : Color.green, 1);

        _lastTimeFired = Time.time;
    }

    private static void OnFireChanged(Changed<WeaponHandler> changed)
    {
        bool IsFiringCurrent = changed.Behaviour.IsFiring;
        
        //Load the old value
        changed.LoadOld();
        bool IsFiringOld = changed.Behaviour.IsFiring;
        
        if(IsFiringCurrent && !IsFiringOld) changed.Behaviour.OnFireRemote();
    }

    private void FireGrenade(Vector3 aimToThrow)
    {
        if (grenadeThrowDelay.ExpiredOrNotRunning(Runner))
        {
            Runner.Spawn(grenadeHandler, _aimPoint.position + aimToThrow * 1.5f, Quaternion.LookRotation(aimToThrow),
                Object.InputAuthority,
                (runner, spawnedGrenade) =>
                {
                    spawnedGrenade.GetComponent<GrenadeHandler>().Throw(aimToThrow * 15, Object.InputAuthority, _networkPlayer.networkedPlayerName.ToString());
                });
            
            grenadeThrowDelay = TickTimer.CreateFromSeconds(Runner, 8f);
        }
    }
    private void OnFireRemote()
    {
        // Play particle on other clients
        if (!Object.HasInputAuthority)
        {
            foreach (ParticleSystem effects in _fireParticleEffect)
            {
                effects.Play();
            }
        }
    }

}
