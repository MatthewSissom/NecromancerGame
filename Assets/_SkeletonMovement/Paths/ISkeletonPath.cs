using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ISkeletonPath
{
    public float Duration { get; protected set; }
    public abstract Vector3 GetPointOnPath(float time);
    public abstract Vector3 GetPointOnPath(float time, out Vector3 forward);
}
