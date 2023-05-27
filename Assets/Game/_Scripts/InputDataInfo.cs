using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public struct InputDataInfo : INetworkSerializeByMemcpy
{
    public bool IsJumpPressed;
    public bool IsSprinting;
    public bool IsShootPressed;
    public Vector2 Movement;
    public Vector3 CameraDirection;
}
