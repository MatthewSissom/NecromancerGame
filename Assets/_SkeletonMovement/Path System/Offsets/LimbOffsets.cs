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
    public class FootData
    {

    }

    private LimbData mData;
    private ISkeletonPath stepPath;
    private float stepEndTime;

    public StepOffset(LimbData data, System.Predicate<FootData> shouldStep)
    {
        mData = data;
    }

    private ISkeletonPath GetStepPath(Vector3 start, Vector3 end)
    {
        SkeletonPathOffset sinOffset = new SinOffset(mData.StepHeight);

        float stepTime = (end - start).magnitude / mData.StepSpeed;
        sinOffset.BasePath = new LinePath(stepTime, start, end);

        return sinOffset;
    }

    protected override Vector3 ApplyOffset(float time, Vector3 inital)
    {
        throw new System.NotImplementedException();
    }
}
