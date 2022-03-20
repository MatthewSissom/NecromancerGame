using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpAction : Action
{
    public JumpAction(Vector3[] destinations, SkeletonBasePathBuilder pathBuilder, SkeletonLayoutData layoutData) : base(destinations, pathBuilder, layoutData)
    {

    }

    public override void MakeActive(Vector3 forward, Vector3 groundPosition)
    {
        throw new System.NotImplementedException();
    }

    public override void MakeActive(IContinuousSkeletonPath path, float traceTime)
    {
        throw new System.NotImplementedException();
    }

    protected override IContinuousSkeletonPath CalculatePath()
    {
        throw new System.NotImplementedException();
    }

    protected override SkeletonPathOffset[] CreateLimbOffsets()
    {
        throw new System.NotImplementedException();
    }

    protected override SkeletonPathOffset[] CreateSpineOffsets()
    {
        throw new System.NotImplementedException();
    }
}
