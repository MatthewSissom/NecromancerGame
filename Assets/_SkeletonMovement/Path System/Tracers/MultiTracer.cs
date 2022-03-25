using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiTracer
{
    protected DelayedTracer[] tracers;

    public bool Update(float dt)
    {
        bool tracing = false;
        foreach (var tracer in tracers)
        {
            tracing |= tracer.Update(dt);
        }
        return tracing;
    }

    public void SetPath(IContinuousSkeletonPath basePath)
    {
        foreach (var tracer in tracers)
        {
            tracer.Path = basePath;
        }
    }

    public void SetOffsets(SkeletonPathOffset[] offsets)
    {
        for (int i = 0; i < tracers.Length; i++)
        {
            tracers[i].SetOffset(offsets[i]);
        }
    }

    public MultiTracer(IDelayedTracerData[] pointData) : this(pointData.Length)
    {
        for (int i = 0; i < tracers.Length; i++)
        {
            tracers[i] = new DelayedTracer(pointData[i].Transform, pointData[i].Delay);
        }
    }

    protected MultiTracer(int len)
    {
        tracers = new DelayedTracer[len];
    }
}
