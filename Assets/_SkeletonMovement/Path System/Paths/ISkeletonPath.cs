using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDuration
{
    float EndTime { get; }
}

public interface INegitiveDuration
{
    float StartTime { get; }
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
    float PositiveDeletedTime { get; }
    // return total time of deleted path
    void DeletePathBefore(float time);
}