using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action
{
    public enum ActionType
    {
        Walk,
        Jump
    }

    public abstract bool Cancelable { get; }
    public IContinuousSkeletonPath Path { get => path ?? (path = CalculatePath()); }
    public SkeletonPathOffset[] LimbOffsets { get => limbOffsets ??  (limbOffsets = CreateLimbOffsets()); }
    public SkeletonPathOffset[] SpineOffsets { get => spineOffsets ?? (spineOffsets  = CreateSpineOffsets()); }
    public virtual ActionType Type { get; }

    private IContinuousSkeletonPath path;
    private SkeletonPathOffset[] limbOffsets;
    private SkeletonPathOffset[] spineOffsets;
    protected Vector3[] destinations;
    protected SkeletonBasePathBuilder pathBuilder;
    protected SkeletonLayoutData layoutData;

    abstract public void MakeActive(Vector3 forward);
    abstract public void MakeActive(IContinuousSkeletonPath path, float traceTime);

    // returns base paths common to most actions
    virtual protected SkeletonPathOffset[] CreateLimbOffsets()
    {
        SkeletonPathOffset[] pathOffsets = new SkeletonPathOffset[layoutData.LimbEnds.Length];

        throw new System.NotImplementedException();
    }
    virtual protected SkeletonPathOffset[] CreateSpineOffsets()
    {
        int count = layoutData.SpinePoints.Length;
        SkeletonPathOffset[] pathOffsets = new SkeletonPathOffset[count];
        for(int i = 0; i < count; i++)
        {
            SpinePointData data = layoutData.SpinePoints[i];
            if (data.Identity.IsShoulder)
            {

            }
        }

        throw new System.NotImplementedException();
    }
    abstract protected IContinuousSkeletonPath CalculatePath();
    protected Action(Vector3[] destinations, SkeletonBasePathBuilder pathBuilder, SkeletonLayoutData layoutData)
    {
        this.destinations = destinations;
        this.pathBuilder = pathBuilder;
        this.layoutData = layoutData;
    }
}