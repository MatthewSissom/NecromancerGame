using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// combines multiple paths into a single path interface
// the path must be continuous (both positions and tangents are = at connection points)
public class CompositePath : IContinuousSkeletonPath , IHigherOrderPath, IRealDuration
{
    public float EndTime { get; private set; }
    public float StartTime { get; private set; }
    public float PositiveDeletedTime { get => Mathf.Max(0, deletedTime + StartTime); }

    private LinkedList<IContinuousSkeletonPath> components;
    private float deletedTime = 0;
    private bool negPathDeleted;

#if UNITY_EDITOR
    private Queue<IContinuousSkeletonPath> deletedComponents;
#endif

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
        StartTime = (components.First.Value as IRealDuration)?.StartTime ?? 0;

        EndTime = 0;
        foreach (var pathComponent in this.components)
        {
            EndTime += pathComponent.EndTime;
        }
    }

    public CompositePath( params IContinuousSkeletonPath[] components) : this(new LinkedList<IContinuousSkeletonPath>(components)) {}

    // get the corresponding path for a given time
    private IContinuousSkeletonPath GetPathForTime(float time, out float skippedPathDuration) 
    {
        skippedPathDuration = 0;
        Assert.IsTrue(time >= deletedTime + StartTime && time <= EndTime, "Time is out of bounds. Possibly trying to access deleated path");

        if(time < 0)
        {
            return components.First.Value;
        }

        if (time == EndTime)
            return components.Last.Value;

        var node = components.First;
        IContinuousSkeletonPath currentPathComponent = node.Value;
        while (node != null)
        {
            // skip paths that don't have enough duration to be accessed
            if (currentPathComponent.EndTime >= time - skippedPathDuration)
            {
                return currentPathComponent;
            }
            else
            {
                node = node.Next;
                skippedPathDuration += currentPathComponent.EndTime;
                currentPathComponent = node?.Value;
            }
        }

        Debug.LogError("Composite path's components have less duration than their sum");
        return null;
    }

    public void DeletePathBefore(float time)
    {
#if UNITY_EDITOR
        deletedComponents = new Queue<IContinuousSkeletonPath>();
#endif

        // delete any components whos end is before time
        while ( components.First.Value.EndTime + deletedTime < time)
        {
            deletedTime += DeleteFirstComponent();
        }

        // check if part of the first component can be deleted
        IHigherOrderPath firstAsHOP = components.First.Value as IHigherOrderPath;
        if (firstAsHOP != null)
        {
            float previousDeletionTime = firstAsHOP.PositiveDeletedTime;
            firstAsHOP.DeletePathBefore(time - deletedTime);
            deletedTime += firstAsHOP.PositiveDeletedTime - previousDeletionTime;
        }

        Assert.IsTrue(StartTime < time, "Deleted too much!");
    }

    public float DeleteFirstComponent()
    {
        IContinuousSkeletonPath toDelete = components.First.Value;
        float timeToDelete = toDelete.EndTime;

        // only count removed negitive time as deleted time for this path's first component
        if (!negPathDeleted)
        {
            IRealDuration firstAsRealDuration = toDelete as IRealDuration;
            if (firstAsRealDuration != null)
            {
                negPathDeleted = true;
                timeToDelete -= firstAsRealDuration.StartTime;
            }
        }

        // don't double count already deleted time
        IHigherOrderPath firstAsHOP = toDelete as IHigherOrderPath;
        if (firstAsHOP != null)
        {
            timeToDelete -= firstAsHOP.PositiveDeletedTime;
        }

#if UNITY_EDITOR
        deletedComponents.Enqueue(components.First.Value);
#endif
        components.RemoveFirst();

        return timeToDelete;
    }
}
