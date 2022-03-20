using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineTracer : DelayedTracer
{
    OrientationTracer orientation;
    public SpineTracer(Transform transform, float delay) : base (transform, delay)
    {
        orientation = new OrientationTracer(transform);
    }

    public override void SetPath(IContinuousSkeletonPath basePath)
    {
        // allow base to modify the path before assigning it to orientation
        base.SetPath(basePath);
        orientation.SetPath(path);
    }

    public override bool Update(float dt)
    {
        orientation.Update(dt);
        return base.Update(dt);
    }
}

public class SpineCoordinator : MultiTracer
{
    public SpineTracer Shoulder { get; private set; }
    public SpineTracer Hip { get; private set; }

    public Vector3? GetPathPos()
    {
        return tracers?[0]?.GetPathPos();
    }

    public SpineCoordinator(SpinePointData[] pointData) : base(pointData.Length)
    {
        for(int i = 0; i < tracers.Length; i++)
        {
            SpineTracer newTracer =  new SpineTracer(pointData[i].Transform, pointData[i].Delay);
            if (pointData[i].IsHip)
                Hip = newTracer;
            if (pointData[i].IsShoulder)
                Shoulder = newTracer;

            tracers[i] = newTracer;
        }
    }
}
