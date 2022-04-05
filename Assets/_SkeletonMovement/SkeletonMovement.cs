using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITraceTimeInfoProvider
{
    float MinTraceTime { get; }
    float BaseTraceTime { get; }
    float MaxTraceTime { get; }
}

//Cat movement is in charge of the low level goals like coordinating limb movment
//Recives a base path from cat behavior
public class SkeletonMovement : ITraceTimeInfoProvider
{
    public IContinuousSkeletonPath Path { get => path; }
    public float TraceTime { get; private set; }
    public float BaseTraceTime { get => TraceTime; }
    public bool ActionIsCancelable { get => (action?.Cancelable ?? true); }
    public bool CanSetAction { get => ActionIsCancelable || ActionCompletion >= 1; }
    public float ActionCompletion {
        get 
        {
            if (action == null)
                return 1;
            return Mathf.Min(1, TraceTime / action.Path.EndTime);
        }
    }

    public float MinTraceTime
    {
        get 
        {
            TracerBase best = FindBestTracer((TracerBase left, TracerBase right) => left.TotalTimeOffset < right.TotalTimeOffset);
            return best.TotalTimeOffset + TraceTime;
        }
    }

    public float MaxTraceTime
    {
        get
        {
            TracerBase best = FindBestTracer((TracerBase left, TracerBase right) => left.TotalTimeOffset > right.TotalTimeOffset);
            return best.TotalTimeOffset + TraceTime;
        }
    }

    private Action action;
    private IContinuousSkeletonPath path;
    FootCoordinator footCoordinator;
    SpineCoordinator spineCoordinator;

    // temp?
    Transform firstSpinePoint;

    //speed and ground height are temp, should be moved here eventually
    public SkeletonMovement(LimbData[] limbEnds, SpinePointData[] spinePoints)
    {
        spineCoordinator = new SpineCoordinator(spinePoints);
        footCoordinator = new FootCoordinator(limbEnds);

        firstSpinePoint = spinePoints[0].Transform;
        SetPath(null);
    }

    public void SetAction(Action newAction)
    {
        if (!CanSetAction)
        {
            Debug.LogError("Set action called incorrectly, always check CanSetAction first");
            return;
        }
        if(newAction == null)
        {
            Debug.LogError("Set null action");
            return;
        }

        // make action active, check for specific transitions before 
        bool isActive = UseSpecialActionTransition(newAction);
        if (!isActive)
        {
            if (action != null)
            {
                newAction.MakeActive(Path, TraceTime);
            }
            else
            {
                newAction.MakeActive(firstSpinePoint.forward);
            }
        }

        SetPath(newAction.Path);
        spineCoordinator.SetOffsets(newAction.SpineOffsets);
        footCoordinator.SetOffsets(newAction.LimbOffsets);
        footCoordinator.Walking = newAction is WalkAction;

        action = newAction;
    }

    public Vector3 GetPathPos()
    {
        Vector3? pathPos = spineCoordinator.GetPathPos();
        if (pathPos != null)
            return pathPos.Value;

        // no path, use raycast to find the ground
        Vector3 rayDirection = new Vector3(0,-1,0);
        Ray ray = new Ray(firstSpinePoint.position + new Vector3(0,.01f,0), rayDirection);
        if (Physics.Raycast(ray, out RaycastHit info))
        {
            return info.point;
        }

        Debug.LogError("Raycast to find ground failed!");
        return new Vector3();
    }

    private bool UseSpecialActionTransition(Action newAction)
    {
        if (action == null)
            return false;

        // make action active
        switch (newAction.Type)
        {
            case Action.ActionType.Walk:
                if (newAction.Type == action.Type)
                {
                    WalkAction newWalk = newAction as WalkAction;
                    WalkAction oldWalk = action as WalkAction;
                    newWalk.MakeActive(oldWalk);
                    return true;
                }
                return false;
            case Action.ActionType.Jump:
                return false;
            default:
                return false;
        }
    }

    private void SetPath(IContinuousSkeletonPath basePath)
    {
        path = basePath;
        footCoordinator.SetPath(basePath);
        spineCoordinator.SetPath(basePath);
        TraceTime = 0;
    }

    private TracerBase FindBestTracer(System.Func<TracerBase,TracerBase,bool> comparator)
    {
        TracerBase bestFoot = footCoordinator.FindBestTracer(comparator);
        TracerBase bestSpine = spineCoordinator.FindBestTracer(comparator);
        return comparator(bestFoot, bestSpine) ? bestFoot : bestSpine;
    }

    public void Update(float dt)
    {
        footCoordinator.Update(dt);
        spineCoordinator.Update(dt);

        float duration = (Path?.EndTime ?? 0);
        if (TraceTime < duration)
            TraceTime += dt;
        else
            TraceTime = duration;
    }
}