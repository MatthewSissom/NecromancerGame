﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// Holds basic bool values about the limb
[Serializable]
public class LimbIdentityData
{
    [field: SerializeField]
    public bool IsStump { get; private set; }
    [field: SerializeField]
    public bool IsSingle { get; private set; }
    [field: SerializeField]
    public bool IsFront { get; private set; }
    [field: SerializeField]
    public bool IsRight { get; private set; }
    public LimbIdentityData(bool isStump, bool isSingle, bool isFront, bool isRight)
    {
        IsStump = isStump;
        IsSingle = isSingle;
        IsFront = isFront;
        IsRight = isRight;
    }
}

// Holds various dimensions and extents of the limb
[Serializable]
public class LimbMeasurements
{
    [field: SerializeField]
    public float StepHeight { get; private set; }
    [field: SerializeField]
    public float TotalLength { get; private set; }
    [field: SerializeField]
    public float OffsetFromSpine { get; private set; }
    [field: SerializeField]
    //the diamater of the circle that the limb can trace on the ground
    public float StrideLength { get; set; }
}

// Holds game objects associated with the limb
[Serializable]
public class LimbTransforms
{
    [field: SerializeField]
    public Transform Target { get; private set; }
    [field: SerializeField]
    public Transform LimbStart { get; private set; }

    public LimbTransforms(Transform start, Transform target)
    {
        LimbStart = start;
        Target = target;
    }
}

[Serializable]
public class LimbTunables
{
    [field: SerializeField]
    public float StepHeightMult { get; private set; }
    [field: SerializeField]
    public float StepTime { get; private set; }
}

public class LimbTracingData
{
    public float Delay { get; private set; }
    public LimbTunables Tunables { get; private set; }
    public float StepTimeInfrontOfSpinePoint { get; private set; }

    // --- Vars below this comment are set during animation --- //
    public float Compleation { get; set; }
    public int TracerId { get; set; }
    public LimbTracingData(float delay, LimbTunables tunables, float stepTimeInfrontOfSpinePoint)
    {
        Delay = delay;
        Tunables = tunables;
        StepTimeInfrontOfSpinePoint = stepTimeInfrontOfSpinePoint;
    }
}

public class LimbData : MonoBehaviour, IDelayedTracerData
{
    // Editor Interface
    [field: SerializeField]
    public LimbIdentityData IdentityData { get; private set; }
    [field: SerializeField]
    public LimbMeasurements Measurements { get; private set; }

    // Script-only interface
    public LimbTransforms Transforms { get; private set; }
    public  LimbTunables Tunables { get; private set; }
    public LimbTracingData TracingData { get; private set; }

    // IDealyedTracerData
    public Transform Transform { get => Transforms.Target.transform; }
    public float Delay { get => TracingData.Delay; }

    // Script constructor
    public void Init( 
        LimbTransforms      transforms, 
        LimbIdentityData    identityData, 
        LimbMeasurements    measurements,
        LimbTunables        tunables,
        LimbTracingData     tracingData
    )
    {
        Transforms      = transforms; 
        IdentityData    = identityData;
        Measurements    = measurements;
        Tunables        = tunables;
        TracingData     = tracingData;
    }

    // Editor/Debug constructor
    public void EditorInit(
        LimbTransforms gameObjects,
        LimbTunables tunables,
        LimbTracingData tracingData
    )
    {
        Transforms = gameObjects;
        Tunables = tunables;
        TracingData = tracingData;
    }
}