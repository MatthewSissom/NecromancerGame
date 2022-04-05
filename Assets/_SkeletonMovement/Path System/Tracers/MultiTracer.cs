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
            tracers[i].Offset = offsets[i];
        }
    }

    public MultiTracer(IDelayedTracerData[] pointData) : this(pointData.Length)
    {
        for (int i = 0; i < tracers.Length; i++)
        {
            tracers[i] = new DelayedTracer(pointData[i].Transform, pointData[i].Delay);
        }
    }
    public DelayedTracer FindBestTracer(System.Func<TracerBase, TracerBase, bool> comparator)
    {
        if (tracers == null || tracers.Length == 0)
            return null;

        DelayedTracer best = tracers[0];
        for(int i = 1; i < tracers.Length; i++)
        {
            best = comparator(best, tracers[0]) ? best : tracers[0];
        }
        return best;
    }

    protected MultiTracer(int len)
    {
        tracers = new DelayedTracer[len];
    }
}
