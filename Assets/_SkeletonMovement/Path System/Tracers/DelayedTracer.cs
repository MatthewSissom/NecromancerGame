using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedTracer : PositionTracer
{
    public float Delay { get; private set; }

    public DelayedTracer(Transform transform, float delay) : base(transform)
    {
        Delay = delay;
    }

    public override void SetPath(IContinuousSkeletonPath basePath)
    {
        // no delay, tracer can opperate like normal
        if (Delay == 0 || basePath == null)
        {
            base.SetPath(basePath);
            return;
        } 

        // trim incoming path to account for the delay
        base.SetPath(new TrimmedPath(basePath, -Delay, basePath.Duration));
    }
}
