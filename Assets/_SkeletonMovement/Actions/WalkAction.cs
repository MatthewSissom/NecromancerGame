using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkAction : Action
{
    public override ActionType Type => ActionType.Walk;
    public override bool Cancelable { get => true; }

    private WalkAction previousWalk = null;
    private Vector3 forward;

    override public void MakeActive( Vector3 forward)
    {
        this.forward = forward;
    }
    public override void MakeActive(IContinuousSkeletonPath path, float traceTime)
    {
        throw new System.NotImplementedException();
    }

    public void MakeActive(WalkAction cancledWalk)
    {
        previousWalk = cancledWalk;
    }

    public WalkAction(Vector3[] destinations, SkeletonBasePathBuilder pathBuilder, SkeletonLayoutData layoutData) : base(destinations,pathBuilder, layoutData) { }

    protected override IContinuousSkeletonPath CalculatePath()
    {
        IContinuousSkeletonPath path;
        if (previousWalk != null)
            path = pathBuilder.SwitchGroundPath(previousWalk.Path, destinations);
        else
            path = pathBuilder.GroundPathFromPoints(forward,destinations);

#if UNITY_EDITOR
        RenderDebugPath(path);
#endif
        return path;
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
            new StepOffset(data.Measurements.StepHeight)
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

#if UNITY_EDITOR
    private void RenderDebugPath(ISkeletonPath path)
    {
        const float simulatedTimeStep = .1f;
        List<Vector3> points = new List<Vector3>();
        float startTime = (path as INegitiveDuration)?.StartTime ?? 0;
        for(float time = startTime + simulatedTimeStep; time < path.EndTime; time+= simulatedTimeStep)
        {
            points.Add(path.GetPointOnPath(time));
        }

        // sample points on path for debugging
        DebugRendering.UpdatePath(DebugModes.DebugPathFlags.TruePath, points);
    }
#endif
}
