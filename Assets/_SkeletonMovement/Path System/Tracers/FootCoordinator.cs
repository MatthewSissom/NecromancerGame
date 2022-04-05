using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public interface IStepInfoProvider
{
    bool LimbShouldStep(LimbTracingData data);
    bool InitLimbShouldStep(LimbTracingData data);
}

public class PairedFootData 
{
    public bool ShouldStep 
    {
        get => tracingData != null && tracingData.Compleation > percentThreshold;
    }

    private LimbTracingData tracingData;
    private float percentThreshold;
    public PairedFootData(LimbTracingData tracingData, float percentThreshold)
    {
        this.tracingData = tracingData;
        this.percentThreshold = percentThreshold;
    }
}

public class FootCoordinator : MultiTracer, IStepInfoProvider
{
    // holds all paired feet
    private Dictionary<int, PairedFootData> idToPairedFoot;
    int maxId;

    public bool Walking
    {
        get
        {
            bool walking = true;
            foreach(var t in tracers)
            {
                walking &= (t as FootTracer)?.Walking ?? false;
            }
            return walking;
        }
        set
        {
            foreach (var t in tracers)
            {
                FootTracer ft = (t as FootTracer);
                if (ft != null)
                    ft.Walking = value;
            }
        }
    }

    public FootCoordinator(LimbData[] pointData) : base(pointData.Length)
    {
        for(int i = 0; i < tracers.Length; i++)
        {
            tracers[i] = new FootTracer(pointData[i], this);
        }

        // init TracerIDs
        int id = 0;
        foreach(var data in pointData)
            data.TracingData.TracerId = id++;
        maxId = id;

        // find paired feet
        // limbs should step when their paired foot is roughly 90% of the way through their step
        idToPairedFoot = new Dictionary<int, PairedFootData>();
        foreach(var data in pointData)
        {
            LimbIdentityData identity = data.IdentityData;
            bool idealIsRight = identity.IsFront ? !identity.IsRight : identity.IsRight;
            bool idealIsFront = !identity.IsFront;

            // Find best pair available
            LimbData pairData;
            // Ideal case
            pairData = Array.Find(pointData, (LimbData candidateData) =>
            {
                LimbIdentityData candidateIdentity = candidateData.IdentityData;
                return candidateData != data
                    && candidateIdentity.IsFront == idealIsFront
                    && candidateIdentity.IsRight == idealIsRight;
            });
            // Runner up
            if (pairData == null)
            {
                pairData = Array.Find(pointData, (LimbData candidateData) =>
                {
                    LimbIdentityData candidateIdentity = candidateData.IdentityData;
                    return candidateData != data
                        && candidateIdentity.IsFront == idealIsFront;
                });
            }
            // Last resort
            if (pairData == null)
            {
                LimbData lastResort = Array.Find(pointData, (LimbData candidateData) =>
                {
                    LimbIdentityData candidateIdentity = candidateData.IdentityData;
                    return candidateData != data;
                });
            }

            LimbIdentityData pairIdentity = pairData.IdentityData;
            // Don't start step before pair finishes their step for special cases
            bool waitForPairCompleation = pairIdentity.IsFront == identity.IsFront
                || pairIdentity.IsSingle
                || identity.IsSingle;
            PairedFootData pair = new PairedFootData(pairData.TracingData, waitForPairCompleation ? 1 : .8f);
            idToPairedFoot.Add(data.TracingData.TracerId, pair);
        }
    }

    // chose a limb to start moving psudo randomly
    public bool InitLimbShouldStep(LimbTracingData data)
    {
        return true;
        return Mathf.FloorToInt(Time.realtimeSinceStartup) % (maxId+1) == data.TracerId;
    }

    public bool LimbShouldStep(LimbTracingData data)
    {
        return false;
        PairedFootData pair = idToPairedFoot[data.TracerId];
        return pair?.ShouldStep ?? false;
    }
}
