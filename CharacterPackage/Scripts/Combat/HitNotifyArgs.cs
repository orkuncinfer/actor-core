using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitNotifyArgs
{
    public ActorBase From;
    public ActorBase Target;
    public int Damage;
    public int Force;
    public Vector3 Position;
    public Vector3 Direction;
    public Vector3 SurfaceNormal;
    public Collider Collider;
}
