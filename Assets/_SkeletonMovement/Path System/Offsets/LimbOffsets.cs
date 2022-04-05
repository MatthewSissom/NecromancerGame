using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepOffset : SkeletonPathOffset
{
    private Vector3 baseVec;
    private float sinDuration;
    private float startTime;

    public void StartStep(float stepStartTime, float stepEndTime)
    {
        startTime = stepStartTime;
        sinDuration = stepEndTime - stepStartTime;
    }

    // called when a new path is created
    public void AdjustStep(float traceTime, float stepDuration)
    {
        startTime -= traceTime;
        sinDuration = stepDuration;
    }

    public void EndStep()
    {
        sinDuration = 0;
    }

    public StepOffset(float maxHeight)
    {
        baseVec = new Vector3(0, maxHeight, 0);
        startTime = 0;
        sinDuration = 0;
    }

    protected override Vector3 ApplyOffset(float time, Vector3 inital)
    {
        if (sinDuration == 0 || time < startTime || time > startTime + sinDuration)
            return inital;

        return inital + baseVec * Mathf.Sin( (time - startTime) / sinDuration * Mathf.PI);
    }
}
