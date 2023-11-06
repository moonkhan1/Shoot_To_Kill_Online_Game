using Cysharp.Threading.Tasks;
using Fusion;
using ModestTree;
using System.Linq;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    public ParticleSystem[] _fireParticleEffects;
    public ParticleSystem[] _remoteFireParticleEffects;
    private float _lastTimeFired = 0;
    private float _maxFireDistance = 150;
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
        bool anyNullElements = _remoteFireParticleEffects.Any(particle => particle == null);
        if (anyNullElements)
        {
            _remoteFireParticleEffects = GetComponentsInChildren<ParticleSystem>();

        }
        _hpHandler.OnDead += () =>
        {
            return;
        };
        if (GetInput(out NetworkInputData networkInputData))
        {
            if (networkInputData.IsFiring)
                Fire(networkInputData.AimForwardVector, networkInputData.LocalCameraPosition);
            if(networkInputData.IsThrown)
                FireGrenade(networkInputData.AimForwardVector);
        }
    }

    private HPHandler CustomFireDirection(Vector3 aimForwardVector, Vector3 cameraPosition, out Vector3 fireDirection)
    {
        LagCompensatedHit lagCompensatedHit = new LagCompensatedHit();
        fireDirection = aimForwardVector;
        float hitDistance = _maxFireDistance;

        if (_networkPlayer.IsThirdPersonCamera)
        {
            Runner.LagCompensation.Raycast(cameraPosition, fireDirection, hitDistance, Object.InputAuthority, out lagCompensatedHit, collisionLayer, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX);

            // hit other players
            if (lagCompensatedHit.Hitbox != null)
            {
                fireDirection = (lagCompensatedHit.Point - _aimPoint.position).normalized;
                hitDistance = lagCompensatedHit.Distance;

                Debug.DrawRay(cameraPosition, aimForwardVector * hitDistance, Color.cyan, 1);
            }
            //hit PhysX collider, not players
            else if (lagCompensatedHit.Collider != null)
            {
                fireDirection = (lagCompensatedHit.Point - _aimPoint.position).normalized;
                hitDistance = lagCompensatedHit.Distance;

                Debug.DrawRay(cameraPosition, aimForwardVector * hitDistance, Color.magenta, 1);
            }
            //hit nothing
            else
            {
                fireDirection = ((cameraPosition + fireDirection * hitDistance) - _aimPoint.position).normalized;
                Debug.DrawRay(cameraPosition, aimForwardVector * hitDistance, Color.grey, 1);

            }

        }

        hitDistance = _maxFireDistance;
        Runner.LagCompensation.Raycast(_aimPoint.position, fireDirection, hitDistance,
            Object.InputAuthority, out lagCompensatedHit, collisionLayer, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX);

        if (lagCompensatedHit.Hitbox != null)
        {
            hitDistance = lagCompensatedHit.Distance;
            HPHandler hitHPHandler = null;

            if (Object.HasStateAuthority)
            {
                hitHPHandler = lagCompensatedHit.Hitbox.transform.root.GetComponent<HPHandler>();
                Debug.DrawRay(_aimPoint.position, fireDirection * hitDistance, Color.blue, 1);
                
                return hitHPHandler;
            }
        }
        else if (lagCompensatedHit.Collider != null)
        {
            hitDistance = lagCompensatedHit.Distance;
            Debug.DrawRay(_aimPoint.position, fireDirection * hitDistance, Color.red, 1);

        }
        Debug.DrawRay(_aimPoint.position, fireDirection * hitDistance, Color.black, 1);
        return null;
    }

    private async void Fire(Vector3 aimForwardVector, Vector3 cameraPosition)
    {
        if(Time.time - _lastTimeFired < 0.3f) return;

        IsFiring = true;
        if (_networkPlayer.IsThirdPersonCamera)
        {
            foreach (ParticleSystem effects in _remoteFireParticleEffects)
            {
                effects.Play();
            }
        }
        else
        {
            foreach (ParticleSystem effects in _fireParticleEffects)
            {
                effects.Play();
            }
        }

        await UniTask.WaitForSeconds(0.09f);
        IsFiring = false;

        HPHandler hitHPHandler = CustomFireDirection(aimForwardVector, cameraPosition, out Vector3 fireDirection);
        
        if(hitHPHandler != null && Object.HasStateAuthority) 
        {
            hitHPHandler.OnTakeDamage(_networkPlayer.networkedPlayerName.ToString(), 1);
        }

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
            foreach (ParticleSystem effects in _remoteFireParticleEffects)
            {
                effects.Play();
            }
        }
    }

}
