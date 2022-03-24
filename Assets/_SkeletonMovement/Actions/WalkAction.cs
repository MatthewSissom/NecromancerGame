using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkAction : Action
{
    public override ActionType Type => ActionType.Walk;

    private WalkAction previousWalk = null;
    private float previousTraceTime = 0;
    private Vector3 forward;
    private Vector3 groundPosition;

    override public void MakeActive( Vector3 forward, Vector3 groundPosition)
    {
        this.forward = forward;
        this.groundPosition = groundPosition;
    }
    public override void MakeActive(IContinuousSkeletonPath path, float traceTime)
    {
        throw new System.NotImplementedException();
    }

    public void MakeActive(WalkAction cancledWalk, float traceTime)
    {
        previousWalk = cancledWalk;
        previousTraceTime = traceTime;
    }

    public WalkAction(Vector3[] destinations, SkeletonBasePathBuilder pathBuilder, SkeletonLayoutData layoutData) : base(destinations,pathBuilder, layoutData) { }

    protected override IContinuousSkeletonPath CalculatePath()
    {
        if (previousWalk != null)
            return pathBuilder.SwitchGroundPath(previousWalk.Path, previousTraceTime, destinations);
        return pathBuilder.GroundPathFromPoints(groundPosition,forward,destinations);
    }

    protected override SkeletonPathOffset[] CreateLimbOffsets()
    {
        if (previousWalk != null)
            return previousWalk.LimbOffsets;

        int cnt = layoutData.LimbEnds.Length;
        SkeletonPathOffset[] offsets = new SkeletonPathOffset[cnt];
        for (int i = 0; i < cnt; i++)
        {
            offsets[i] = CreateLimbOffsetFromData(layoutData.LimbEnds[i]);
        }
        return offsets;
    }

    private SkeletonPathOffset CreateLimbOffsetFromData(LimbData data)
    {
        return new CompositeOffsite(
            new PerpendicularOffset(data.Measurements.OffsetFromSpine * (data.IdentityData.IsRight ? -1 : 1)),
            new HeightOffset(0)
            );
    }

    protected override SkeletonPathOffset[] CreateSpineOffsets()
    {
        if (previousWalk != null)
            return previousWalk.SpineOffsets;

        int cnt = layoutData.SpinePoints.Length;
        SkeletonPathOffset[] offsets = new SkeletonPathOffset[cnt];
        for(int i = 0; i < cnt; i++)
        {
            offsets[i] = CreateSpineOffsetFromData(layoutData.SpinePoints[i]);
        }
        return offsets;
    }

    private SkeletonPathOffset CreateSpineOffsetFromData(SpinePointData data)
    {
        return new CompositeOffsite(
            new HeightOffset(data.BaseHeight),
            new HeightOffset(0)
            );
    }
}
