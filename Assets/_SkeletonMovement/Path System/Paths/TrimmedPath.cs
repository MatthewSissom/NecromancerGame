using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// used to shorten the duration of a path without modifying the underlying path
public class TrimmedPath : IContinuousSkeletonPath, IHigherOrderPath, IRealDuration
{
    public float EndTime { get; private set; }
    public float StartTime { get; private set; }
    public float PositiveDeletedTime { get => Mathf.Max(0, deletedTime + StartTime - zeroTime); }

    private IContinuousSkeletonPath basePath;
    private float zeroTime;
    private float deletedTime = 0;

    public Vector3 GetPointOnPath(float time)
    {
        return basePath.GetPointOnPath(time + zeroTime);
    }

    public Vector3 GetTangent(float time)
    {
        return basePath.GetTangent(time + zeroTime);
    }

    public Vector3 GetForward(float time)
    {
        return basePath.GetForward(time + zeroTime);
    }

    public void DeletePathBefore(float time)
    {
        IHigherOrderPath baseAsHOP = basePath as IHigherOrderPath;
        if (baseAsHOP == null)
            return;
        float previousDeletedTime = baseAsHOP.PositiveDeletedTime;
        DeletePathBefore(time + zeroTime);
        deletedTime += baseAsHOP.PositiveDeletedTime - previousDeletedTime;
    }

    public TrimmedPath(IContinuousSkeletonPath toTrim, float zeroTime, float endTime)
    {
        basePath = toTrim;
        this.zeroTime = zeroTime;

        // see if base path has negitive duration
        IRealDuration toTrimNegitiveGetter = basePath as IRealDuration;
        float baseNegDuration = (toTrimNegitiveGetter?.StartTime) ?? 0;
        StartTime = baseNegDuration - zeroTime;
        EndTime = Mathf.Min(endTime + zeroTime, toTrim.EndTime) - zeroTime;
    }
}