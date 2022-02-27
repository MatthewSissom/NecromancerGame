using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// combines multiple paths into a single path interface
// the path must be continuous (both positions and tangents are = at connection points)
public class CompositePath : IContinuousSkeletonPath
{
    public float Duration { get; private set; }

    private LinkedList<IContinuousSkeletonPath> components;

    public Vector3 GetPointOnPath(float time)
    {
        IContinuousSkeletonPath path = GetPathForTime(time, out float skippedTime);
        return path.GetPointOnPath(time - skippedTime);
    }

    Vector3 IContinuousPath.GetTangent(float time)
    {
        IContinuousSkeletonPath path = GetPathForTime(time, out float skippedTime);
        return path.GetTangent(time - skippedTime);
    }

    Vector3 IContinuousPath.GetForward(float time)
    {
        IContinuousSkeletonPath path = GetPathForTime(time, out float skippedTime);
        return path.GetForward(time - skippedTime);
    }

    public CompositePath( LinkedList<IContinuousSkeletonPath> components)
    {
        this.components = components;

        Duration = 0;
        foreach (var pathComponent in this.components)
        {
            Duration += pathComponent.Duration;
        }
    }

    // get the corresponding path for a given time
    private IContinuousSkeletonPath GetPathForTime(float time, out float skippedPathDuration) 
    {
        skippedPathDuration = 0;
        if (time > Duration)
            return null;

        var node = components.First;
        IContinuousSkeletonPath currentPathComponent = node.Value;
        while (node != null)
        {
            // skip paths that don't have enough duration to be accessed
            if (currentPathComponent.Duration >= time - skippedPathDuration)
            {
                return currentPathComponent;
            }
            else
            {
                node = node.Next;
                skippedPathDuration += currentPathComponent.Duration;
                currentPathComponent = node?.Value;
            }
        }

        Debug.LogError("Composite path's components have less duration than their sum");
        return null;
    }
}
