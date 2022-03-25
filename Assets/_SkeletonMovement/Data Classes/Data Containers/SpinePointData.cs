using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineIdentityData
{
    public bool IsShoulder { get; private set; }
    public bool IsHip { get; private set; }

    public SpineIdentityData(bool isShoulder, bool isHip)
    {
        IsShoulder = isShoulder;
        IsHip = isHip;
    }
}

public class SpinePointData : IDelayedTracerData
{
    public Transform Transform { get; private set; }
    // delays should not be negitive
    public float Delay { get; private set; }
    public SpineIdentityData Identity { get; private set; }
    public float BaseHeight { get; private set; }

    public SpinePointData(Transform transform, float delay, SpineIdentityData IdData)
    {
        Transform = transform;
        Delay = delay;
        if (delay < 0)
            Debug.LogError("Spine delays cannot be less than 0!");
        Identity = IdData;
    }
}
