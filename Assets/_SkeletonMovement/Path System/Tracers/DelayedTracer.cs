using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DelayedTracer : PositionTracer
{
    public override float TotalTimeOffset { get => base.TotalTimeOffset - Delay; }
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
        base.SetPath(new TrimmedPath(basePath, -Delay, basePath.EndTime));
    }
}

public class TimeScaleTracer : DelayedTracer
{
    public float TimeScale { get; protected set; } = 1;
    protected float TimeOffsetFromBase { get; set; }
    public override float TotalTimeOffset => base.TotalTimeOffset + TimeOffsetFromBase;

    public override void SetPath(IContinuousSkeletonPath basePath)
    {
        base.SetPath(basePath);
        traceTime = TimeOffsetFromBase;
    }
    public override bool Update(float dt)
    {
        TimeOffsetFromBase += dt*(TimeScale-1);
        return base.Update(dt * TimeScale);
    }
    public TimeScaleTracer(Transform transform, float delay) : base(transform, delay){ }
}