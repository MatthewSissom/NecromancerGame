using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDuration
{
    float Duration { get; }
}

public interface INegitiveDuration
{
    float NegitiveDuration { get; }
}

public interface IRealDuration : IDuration, INegitiveDuration {}

public interface ISkeletonPath : IDuration
{
    Vector3 GetPointOnPath(float time);
}

public interface IContinuousPath : ISkeletonPath
{
    // left tangent of path
    Vector3 GetTangent(float time);
    Vector3 GetForward(float time);
}

public interface IContinuousSkeletonPath : ISkeletonPath, IContinuousPath { }

// paths that use other paths in their calculations
public interface IHigherOrderPath
{
    void DeletePathBefore(float time);
}