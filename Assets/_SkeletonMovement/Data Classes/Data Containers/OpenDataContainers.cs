using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Holds basic bool values about the limb
public class OpenLimbIdentityData
{
    [field: SerializeField]
    public bool IsStump { get;   set; }
    [field: SerializeField]
    public bool IsSingle { get;   set; }
    [field: SerializeField]
    public bool IsFront { get;   set; }
    [field: SerializeField]
    public bool IsRight { get;   set; }

    public OpenLimbIdentityData(bool isFront, bool isRight)
    {
        IsFront = isFront;
        IsRight = isRight;
        IsSingle = false;
        IsStump = false;
    }

    public OpenLimbIdentityData() : this(false, false) { }
}

// Holds various dimensions and extents of the limb
public class OpenLimbMeasurements
{
    [field: SerializeField]
    public float StepHeight { get;   set; }
    [field: SerializeField]
    public float TotalLength { get;   set; }
    [field: SerializeField]
    public float OffsetFromSpine { get;   set; }
    [field: SerializeField]
    //the diamater of the circle that the limb can trace on the ground
    public float StrideLength { get; set; }
}

// Holds game objects associated with the limb
public class OpenLimbTransforms
{
    [field: SerializeField]
    public Transform Target { get;   set; }
    [field: SerializeField]
    public Transform LimbStart { get;   set; }
}

public class OpenLimbTracingData
{
    public float Delay { get;   set; }
    public LimbTunables Tunables { get;   set; }
    public float StepTimeInfrontOfSpinePoint { get;   set; }
}

public class OpenLimbData
{
    // Editor Interface
    [field: SerializeField]
    public OpenLimbIdentityData IdentityData { get; set; }
    [field: SerializeField]
    public OpenLimbMeasurements Measurements { get; set; }

    // Script-only interface
    public OpenLimbTransforms Transforms { get; set; }
    public LimbTunables Tunables { get; set; }
    public OpenLimbTracingData TracingData { get; set; }
}

public class OpenSpineIdentityData
{
    public OpenSpineIdentityData()
    {
        IsShoulder = false;
        IsHip = false;
    }

    public OpenSpineIdentityData(bool isShoulder, bool isHip)
    {
        IsShoulder = isShoulder;
        IsHip = isHip;
    }

    public bool IsShoulder { get;   set; }
    public bool IsHip { get;   set; }
}

public class OpenSpinePointData
{
    public Transform Target { get;   set; }
    // delays should not be negitive
    public float Delay { get;   set; }
    public OpenSpineIdentityData Identity { get;   set; }
    public float BaseHeight { get;   set; }
}
