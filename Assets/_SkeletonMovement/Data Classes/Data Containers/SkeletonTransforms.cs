using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class SkeletonTransforms
{
    // Spine transforms should be at the END of their gameObject, tip of the nose, tip of the tail, etc.
    // Values should be null if 
    [field: SerializeField]
    public Transform Head { get; private set; }
    [field: SerializeField]
    public Transform Shoulder { get; private set; }
    [field: SerializeField]
    public Transform Hip { get; private set; }
    [field: SerializeField]
    public Transform Tail { get; private set; }
    // Ordered from head to tail
    [field:SerializeField]
    public Transform[] OrderedSpineTransforms { get; private set; }

    // Leg transforms should correspond to the transform coming off of the shoulder or the hips excluding offsets
    [field: SerializeField]
    public Transform FrontLeftLeg { get; private set; }
    [field: SerializeField]
    public Transform FrontRightLeg { get; private set; }
    [field: SerializeField]
    public Transform BackLeftLeg { get; private set; }
    [field: SerializeField]
    public Transform BackRightLeg { get; private set; }

    public SkeletonTransforms(
        Transform head,
        Transform shoulder,
        Transform hip,
        Transform tail,
        Transform[] orderedSpineTransforms,
        Transform frontLeftLeg,
        Transform frontRightLeg,
        Transform backLeftLeg,
        Transform backRightLeg
        )
    {
        Head = head;
        Shoulder = shoulder;
        Hip = hip;
        Tail = tail;
        OrderedSpineTransforms = orderedSpineTransforms;
        FrontLeftLeg = frontLeftLeg;
        FrontRightLeg = frontRightLeg;
        BackLeftLeg = backLeftLeg;
        BackRightLeg = backRightLeg;
    }
}


[Serializable]
public class SkeletonPathTunables
{
    [field: SerializeField]
    public float MinTurningRad { get; private set; }
    [field: SerializeField]
    public float Speed { get; private set; }

    public SkeletonPathTunables(float minTurningRad, float speed)
    {
        MinTurningRad = minTurningRad;
        Speed = speed;
    }
}

public class SkeletonPathData
{
    public float SkeletonDuration { get; private set; }
    public float DelayedPathLenght { get; private set; }

    public SkeletonPathData(float skeletonDuration, float delayedPathLenght)
    {
        SkeletonDuration = skeletonDuration;
        DelayedPathLenght = delayedPathLenght;
    }

    public SkeletonPathData( SkeletonPathTunables tunables, SkeletonLayoutData layoutData)
    {
        SkeletonDuration = layoutData.SkeletonLenght / tunables.Speed + .25f;
        DelayedPathLenght = SkeletonDuration * tunables.Speed;
    }
}

public class SkeletonLayoutData
{
    public LimbData[] LimbEnds { get; private set; }
    public SpinePointData[] SpinePoints { get; private set; }
    public float SkeletonLenght { get; private set; }

    public SkeletonLayoutData(LimbData[] limbEnds, SpinePointData[] spinePoints, float skeletonLength)
    {
        LimbEnds = limbEnds;
        SpinePoints = spinePoints;
        SkeletonLenght = skeletonLength;
    }
}