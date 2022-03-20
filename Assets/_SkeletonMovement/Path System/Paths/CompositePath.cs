using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// combines multiple paths into a single path interface
// the path must be continuous (both positions and tangents are = at connection points)
public class CompositePath : IContinuousSkeletonPath , IHigherOrderPath, IRealDuration
{
    public float Duration { get; private set; }
    public float NegitiveDuration { get; private set; }

    private LinkedList<IContinuousSkeletonPath> components;
    // allows composite path to have values at negitive time
    private float deletedPathDuration;

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

    public CompositePath( LinkedList<IContinuousSkeletonPath> components, float negitiveDuration = 0)
    {
        this.components = components;
        NegitiveDuration = negitiveDuration;
        deletedPathDuration = 0;

        Duration = 0;
        foreach (var pathComponent in this.components)
        {
            Duration += pathComponent.Duration;
        }
        // remove negitive duration from total
        Duration += negitiveDuration;
    }

    public CompositePath( params IContinuousSkeletonPath[] components) : this(new LinkedList<IContinuousSkeletonPath>(components)) {}

    // get the corresponding path for a given time
    private IContinuousSkeletonPath GetPathForTime(float time, out float skippedPathDuration) 
    {
        skippedPathDuration = NegitiveDuration + deletedPathDuration;

        if (time < skippedPathDuration + NegitiveDuration)
        {
            Debug.LogError("Trying to access deleated path");
            return null;
        }

        if (time > Duration || components.Count == 0)
            return null;

        if (time == Duration)
            return components.Last.Value;

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

    public void DeletePathBefore(float time)
    {
        float accumulatedTime = NegitiveDuration + deletedPathDuration;
        while (accumulatedTime + components.First.Value.Duration < time)
        {
            accumulatedTime += components.First.Value.Duration;
            deletedPathDuration += components.First.Value.Duration;
            components.RemoveFirst();
        }
        (components.First.Value as IHigherOrderPath)?.DeletePathBefore(time - accumulatedTime);
    }
}
