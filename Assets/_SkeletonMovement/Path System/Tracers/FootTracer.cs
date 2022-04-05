using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootTracer : TimeScaleTracer
{
    private enum LimbState
    {
        Tracing, // Not walking, act like any normal tracer
        Standing,// Walking but not stepping
        Stepping,// Lifting up and moving forwards
        Init     // Walking but not yet initalized
    }

    public bool Walking
    {
        get => state == LimbState.Standing || state == LimbState.Stepping;
        set => state = Walking ? state
            : (value && !traceLock) ? LimbState.Init : LimbState.Tracing;
    }

    // Interaction with other classes
    private IStepInfoProvider stepInfo;
    private StepOffset stepOffset;

    // State
    private LimbState state;
    private bool traceLock;     // locked to tracing (no step offset)
    private float stepStartTime;
    private float stepEndTime;

    // Data
    private LimbTracingData tracingData;
    private LimbTransforms transforms;
    private float totalLimbLenght;

    public FootTracer(LimbData data, IStepInfoProvider stepInfo) : base(data.Transforms.Target, data.TracingData.Delay)
    {
        tracingData = data.TracingData;
        transforms = data.Transforms;
        totalLimbLenght = data.Measurements.TotalLength;
        this.stepInfo = stepInfo;

        traceLock = true;
        Walking = false;
        stepEndTime = 0;
    }

    public override void SetPath(IContinuousSkeletonPath basePath)
    {
        // adjust times to be in terms of the new path
        stepStartTime -= traceTime;
        stepEndTime -= traceTime;
        if(basePath != null)
            stepEndTime = Mathf.Min(stepEndTime, basePath.EndTime);

        // update offset with new times
        if(stepOffset != null)
            stepOffset.AdjustStep(traceTime,stepEndTime-stepStartTime);

        base.SetPath(basePath);
    }

    public override bool Update(float dt)
    {
        switch (state)
        {
            case LimbState.Tracing:
                if (TimeOffsetFromBase == 0 && TimeScale == 1)
                    return base.Update(dt);
                if (Mathf.Abs(TimeOffsetFromBase) < .001f)
                {
                    TimeOffsetFromBase = 0;
                    TimeScale = 1;
                    return base.Update(dt);
                }

                // Adjust timeScale to make up time debt if needed
                float scaleBase = 2;
                float scaleEasing = 5;
                if (TimeOffsetFromBase > 0)
                    TimeScale = 1 + scaleBase * Mathf.Atan(TimeOffsetFromBase * scaleEasing);
                else
                    TimeScale = 1 - scaleBase * Mathf.Atan(TimeOffsetFromBase * scaleEasing);
                return base.Update(dt);

            case LimbState.Standing:
                // Check if a new step should be started
                float extension = (transforms.LimbStart.position - transforms.Target.position).magnitude;
                if (
                    stepInfo.LimbShouldStep(tracingData)
                    || (extension > totalLimbLenght && TimeOffsetFromBase < 0)
                )
                {
                    StartStep();
                }
                return base.Update(dt);

            case LimbState.Stepping:
                // Check if step is over
                if (traceTime + dt > stepEndTime)
                {
                    // set dt to be exactly == to remaining time if done stepping
                    float remainingStepTime = (stepEndTime - traceTime) / TimeScale;
                    TimeOffsetFromBase += remainingStepTime - dt;
                    dt = remainingStepTime;

                    bool pathing = base.Update(dt);
                    EndStep();
                    return pathing;
                }
                tracingData.Compleation = (traceTime - stepStartTime) / (stepEndTime - stepStartTime);
                return base.Update(dt);

            case LimbState.Init:
                // Set a true state, then call this method again
                if (stepInfo.InitLimbShouldStep(tracingData))
                    StartStep();
                else
                    EndStep();
                return Update(dt);

            default:
                return base.Update(dt);
        }
    }

    protected override void SetOffset(SkeletonPathOffset offset)
    {
        CompositeOffsite compOffset = offset as CompositeOffsite;
        stepOffset = compOffset.FindComponent<StepOffset>();
        traceLock = stepOffset == null;
        base.SetOffset(offset);
    }

    private void StartStep()
    {
        state = LimbState.Stepping;

        stepStartTime = traceTime;

        stepEndTime = tracingData.StepTimeInfrontOfSpinePoint + traceTime - TimeOffsetFromBase;
        stepEndTime += tracingData.Tunables.StepTime;
        stepEndTime = Mathf.Min(stepEndTime, path.EndTime);

        TimeScale = (stepEndTime-stepStartTime) / tracingData.Tunables.StepTime;

        stepOffset.StartStep(traceTime, tracingData.Tunables.StepTime);
    }

    private void EndStep()
    {
        tracingData.Compleation = 0;
        state = LimbState.Standing;
        TimeScale = 0;
        stepOffset.EndStep();
    }
}
