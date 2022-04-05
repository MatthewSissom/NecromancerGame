using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracerBase
{
    public float Completion { get => Path != null ? traceTime / Path.EndTime : 1; }
    public IContinuousSkeletonPath Path { get => path; set => SetPath(value); }
    // used for comparing path times between tracers
    public virtual float TotalTimeOffset { get => 0; }

    protected IContinuousSkeletonPath path = null;
    protected float traceTime;
    protected Transform transform = null;

    public Vector3? GetPathPos()
    {
        if(Path != null)
            return Path.GetPointOnPath(traceTime);
        return null;
    }

    public Vector3 GetPos()
    {
        return transform.position;
    }

    public virtual void SetPath(IContinuousSkeletonPath basePath)
    {
        traceTime = 0;
        path = basePath;
    }

    // returns true if tracing, false if done tracing
    public virtual bool Update(float dt)
    {
        // path is already finished, nothing to do
        if (Completion >= 1)
            return false;

        // update time and check for finished path
        traceTime += dt;
        if (traceTime >= Path.EndTime)
        {
            FinishPath();
            return false;
        }

        return true;
    }

    protected virtual void FinishPath()
    {
        traceTime = Path.EndTime;
    }

    public TracerBase(Transform transform)
    {
        this.transform = transform;
    }
}

public class PositionTracer : TracerBase
{
    public SkeletonPathOffset Offset { get => offset; set => SetOffset(value); }
    protected SkeletonPathOffset offset = null;

    public override void SetPath(IContinuousSkeletonPath basePath) 
    {
        base.SetPath(basePath);
        if(Offset != null)
            Offset.BasePath = basePath;
    }

    protected virtual void SetOffset(SkeletonPathOffset offset) 
    {
        this.offset = offset;
        if (path != null)
            offset.BasePath = Path;
    }

    public override bool Update(float dt)
    {
        if (!base.Update(dt))
            return false;

        // trace path
        if (Offset != null)
            transform.position = Offset.GetPointOnPath(traceTime);
        else
            transform.position = Path.GetPointOnPath(traceTime);

        return true;
    }

    protected override void FinishPath() 
    {
        // trace path
        if (Offset != null)
            transform.position = Offset.GetPointOnPath(Path.EndTime);
        else
            transform.position = Path.GetPointOnPath(Path.EndTime);

        base.FinishPath();

        if (Offset != null)
            Offset.BasePath = null;
    }

    public PositionTracer(Transform transform) : base(transform) { }
}

public class OrientationTracer : TracerBase
{
    public override bool Update(float dt)
    {
        if (!base.Update(dt))
            return false;

        transform.forward = path.GetForward(traceTime);
        return true;
    }

    protected override void FinishPath()
    {
        transform.forward = path.GetForward(Path.EndTime);
        base.FinishPath();
    }

    public OrientationTracer(Transform transform) : base(transform) { }
}