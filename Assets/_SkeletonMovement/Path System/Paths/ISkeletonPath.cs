using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISkeletonPath
{
    float Duration { get; }
    Vector3 GetPointOnPath(float time);
}

public interface IContinuousPath
{
    // left tangent of path
    Vector3 GetTangent(float time);
    Vector3 GetForward(float time);
}

public interface IContinuousSkeletonPath : ISkeletonPath, IContinuousPath { }