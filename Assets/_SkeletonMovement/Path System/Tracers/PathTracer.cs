using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTracer
{
    public float Completion { get => Path != null ? Path.Duration / traceTime : 1; }
    public IContinuousSkeletonPath Path { get => path; set => SetPath(value); }
    public SkeletonPathOffset Offset { get => offset; set => SetOffset(value); }

    protected IContinuousSkeletonPath path = null;
    protected SkeletonPathOffset offset = null;
    protected float traceTime;
    protected Transform transform = null;

    public virtual void SetPath(IContinuousSkeletonPath basePath) 
    {
        traceTime = 0;
        path = basePath;
        if(Offset != null)
            Offset.BasePath = basePath;
    }

    public void SetOffset(SkeletonPathOffset offset) 
    {
        this.offset = offset;
        if (path != null)
            offset.BasePath = Path;
    }

    public virtual void Update(float dt)
    {
        // path is already finished, nothing to do
        if (Completion == 1)
            return;

        // update time and check for finished path
        traceTime += dt;
        if (traceTime >= Path.Duration)
        {
            FinishPath();
            return;
        }

        // trace path
        if (Offset != null)
            transform.position = Offset.GetPointOnPath(traceTime);
        else
            transform.position = Path.GetPointOnPath(traceTime);
    }

    private void FinishPath() 
    {
        transform.position = Offset.GetPointOnPath(Path.Duration);
        Path = null;
        Offset.BasePath = null;
    }

    public PathTracer(Transform transform)
    {
        this.transform = transform;
    }
}

public class OrientedPathTracer : PathTracer
{
    public OrientedPathTracer(Transform transform) : base(transform) { }

    public override void Update(float dt)
    {
        // path is already finished, nothing to do
        if (Completion == 1)
            return;

        base.Update(dt);
        if(path != null)
            transform.forward = path.GetForward(dt);
    }
}

public class SpineTracer : OrientedPathTracer
{
    public float Delay { get; private set; }

    private IContinuousSkeletonPath unalteredPath;

    public SpineTracer(Transform transform, float delay) : base(transform)
    {
        Delay = delay;
    }

    public override void SetPath(IContinuousSkeletonPath basePath)
    {
        // no delay, tracer can opperate like normal
        if (Delay == 0)
        {
            base.SetPath(basePath);
            return;
        }

        IContinuousSkeletonPath beforeDelay = null;
        // follow unaltered path until delay is over
        if (unalteredPath != null)
        {
            beforeDelay = new TrimmedPath(unalteredPath, unalteredPath.Duration - Delay, Delay);
        }
        // if not currently following a path, use a line instead
        else
        {
            beforeDelay = new LinePath(Delay, basePath.GetPointOnPath(0), Path.GetPointOnPath(traceTime));
        }

        // trim incoming path to account for the delay
        IContinuousSkeletonPath afterDelay = new TrimmedPath(basePath, 0, basePath.Duration - Delay);

        unalteredPath = basePath;
        base.SetPath(new CompositePath(beforeDelay, afterDelay));
    }
}
