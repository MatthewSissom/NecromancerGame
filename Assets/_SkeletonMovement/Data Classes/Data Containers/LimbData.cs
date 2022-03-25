using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// Holds basic bool values about the limb
[Serializable]
public class LimbIdentityData
{
    [field: SerializeField]
    public bool IsStump { get; set; }
    [field: SerializeField]
    public bool IsSingle { get; set; }
    [field: SerializeField]
    public bool IsFront { get; set; }
    [field: SerializeField]
    public bool IsRight { get; set; }
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
    public float StrideLength { get; private set; }
}

// Holds game objects associated with the limb
[Serializable]
public class LimbGameObjects
{
    [field: SerializeField]
    public GameObject Target { get; private set; }
    [field: SerializeField]
    public GameObject LimbStart { get; private set; }
}

[Serializable]
public class LimbTunables
{
    [field: SerializeField]
    public float StepHeightMult;
    public float StepSpeed { get; private set; } = 0;

    [SerializeField]
    private float stepSpeedMult;

    public void Init(float speed)
    {
        StepSpeed = stepSpeedMult * speed;
    }
}

public class LimbTracingData
{
    public float Delay { get; private set; }

    public LimbTracingData(float delay)
    {
        Delay = delay;
    }
}

public class LimbData : MonoBehaviour, IDelayedTracerData
{
    // Editor Interface
    [field: SerializeField]
    public LimbGameObjects GameObjects { get; private set; }
    [field: SerializeField]
    public LimbIdentityData IdentityData { get; private set; }
    [field: SerializeField]
    public LimbMeasurements Measurements { get; private set; }

    // Script-only interface
    public  LimbTunables Tunables { get; private set; }
    public LimbTracingData TracingData { get; private set; }

    // IDealyedTracerData
    public Transform Transform { get => GameObjects.Target.transform; }
    public float Delay { get => TracingData.Delay; }

    // Script constructor
    public void Init( 
        LimbGameObjects GameObjects, 
        LimbIdentityData IdentityData, 
        LimbMeasurements Measurements,
        LimbTunables Tunables,
        LimbTracingData TracingData
    )
    {
       this.GameObjects = GameObjects;
       this.IdentityData = IdentityData;
       this.Measurements = Measurements;
       this.Tunables = Tunables;
       this.TracingData = TracingData;
    }

    // Editor/Debug constructor
    public void EditorInit(
        LimbTunables Tunables,
        LimbTracingData TracingData
    )
    {
        this.Tunables = Tunables;
        this.TracingData = TracingData;
    }
}