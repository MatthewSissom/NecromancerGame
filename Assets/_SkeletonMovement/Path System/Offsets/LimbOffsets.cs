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
        public enum LimbStates
        {
            Tracing,
            Pinned
        }

        public LimbStates LimbState { get; private set; }
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
        SkeletonPathOffset sinOffset = new SinOffset(mData.Measurements.StepHeight);

        float stepTime = (end - start).magnitude / mData.Tunables.StepSpeed;
        sinOffset.BasePath = new LinePath(stepTime, start, end);

        return sinOffset;
    }

    protected override Vector3 ApplyOffset(float time, Vector3 inital)
    {
        throw new System.NotImplementedException();

        //FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Cats/Footsteps/SkeletonFootsteps");

        //private IEnumerator StepRoutine()
        //{
        //    //final position of foot relitive to the origin
        //    Vector3 inital = Target.transform.position;
        //    float elapsedTime = 0;
        //    float percentFinished = 0;
        //    float stepTime = (stepTargetPos - inital).magnitude / StepSpeed;
        //    if (stepTime == 0)
        //    {
        //        yield break;
        //    }
        //    float initalHOffset = HeightOffset;
        //    while (percentFinished < 1)
        //    {
        //        elapsedTime += Time.deltaTime;
        //        percentFinished = elapsedTime / stepTime;

        //        HeightOffset = initalHOffset * (1 - percentFinished);

        //        Target.transform.position = Vector3.Lerp(inital, stepTargetPos, percentFinished)
        //            + new Vector3(0, Mathf.Sin(percentFinished * Mathf.PI) * StepHeight, 0);
        //        yield return null;
        //    }
        //    Target.transform.position = stepTargetPos;
        //    yield break;
        //}
    }
}
