using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3 MovementInput { get; set; }
    public Vector3 AimForwardVector { get; set; }
    public NetworkBool IsJumping { get; set; }
    public NetworkBool IsFiring { get; set; }
    public NetworkBool IsThrown { get; set; }

}
