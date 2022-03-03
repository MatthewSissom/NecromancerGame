using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTracer
{
    public float Completion { 
        get 
        {
            return Path != null ? Path.Duration / traceTime : 1;
        }
    }
    public ISkeletonPath Path { get; private set; }


    private float traceTime;
    private Transform transform;

    public void SetPath(ISkeletonPath toFollow) 
    {
        traceTime = 0;
        Path = toFollow;
    }

    public void Update(float dt)
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
        transform.position = Path.GetPointOnPath(Path.Duration);
    }

    private void FinishPath() 
    {
        transform.position = Path.GetPointOnPath(Path.Duration);
        Path = null;
    }

    public PathTracer(Transform transform)
    {
        this.transform = transform;
    }
}
