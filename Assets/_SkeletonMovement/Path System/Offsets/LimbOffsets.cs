using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinOffset : SkeletonPathOffset
{
    private Vector3 baseVec;
    public SinOffset(float maxHeight)
    {
        baseVec = new Vector3(0,maxHeight,0);
    }
    protected override Vector3 ApplyOffset(float time, Vector3 inital)
    {
        return inital + baseVec * Mathf.Sin(time / Duration * Mathf.PI);
    }
}

public class StepOffset : SkeletonPathOffset
{
    public enum LimbStates
    {
        Tracing,
        Pinned,
        Init
    }

    private LimbData mData;
    private LimbStates state;

    private ISkeletonPath stepPath;
    private float stepStartTime;

    private Vector3 pinPos;

    public StepOffset(LimbData data /*,System.Predicate<FootData> shouldStep*/)
    {
        mData = data;
        state = LimbStates.Init;
    }

    private ISkeletonPath GetStepPath(Vector3 start, Vector3 end, float stepTime)
    {
        SkeletonPathOffset sinOffset = new SinOffset(mData.Measurements.StepHeight);
        sinOffset.BasePath = new LinePath(stepTime, start, end);

        return sinOffset;
    }

    protected override Vector3 ApplyOffset(float time, Vector3 inital)
    {
        switch (state)
        {
            case LimbStates.Tracing:
                float stepPathTime = time - stepStartTime;
                stepPathTime = Mathf.Min(stepPath.Duration, stepPathTime);

                if (stepStartTime == stepPath.Duration)
                    return StartPin(stepPath.GetPointOnPath(stepPathTime));

                return inital + stepPath.GetPointOnPath(stepPathTime);
            case LimbStates.Pinned:

                var transforms = mData.Transforms;
                if ((transforms.LimbStart.position - transforms.Target.position).magnitude >= mData.Measurements.TotalLength)
                    StartStep(time);

                break;
            case LimbStates.Init:
                break;
            default:
                break;
        }

        return new Vector3();
    }

    Vector3 StartStep(float  time)
    {
        throw new System.NotImplementedException();
    }

    Vector3 StartPin(Vector3 newPos)
    {
        pinPos = newPos;
        return pinPos;
    }
}
