using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class SkeletonPathOffset : ISkeletonPath
{
    virtual public ISkeletonPath BasePath { get; set; } = default;
    public float EndTime { get { return BasePath.EndTime; } }

    public Vector3 GetPointOnPath(float time)
    {
        return ApplyOffset(time, BasePath.GetPointOnPath(time));
    }

    abstract protected Vector3 ApplyOffset(float time, Vector3 inital);
}

public class HeightOffset : SkeletonPathOffset
{
    readonly float height;
    public HeightOffset(float height)
    {
        this.height = height;
    }

    protected override Vector3 ApplyOffset(float time, Vector3 inital)
    {
        return inital + new Vector3(0, height, 0);
    }
}

public class PerpendicularOffset : SkeletonPathOffset
{
    public override ISkeletonPath BasePath 
    { 
        get => base.BasePath;
        set
        {
            base.BasePath = value;
            perpGetter = BasePath as IContinuousPath;
            if (perpGetter == null)
            {
                Debug.LogError("Cannot apply perpendicular offset to a non-continuous path. " +
                    "Try moving it earlier in the offset chain."
                    );
            }
        }
    }

    private IContinuousPath perpGetter;
    private readonly float mult;

    public PerpendicularOffset(float leftPerpMultiplier)
    {
        mult = leftPerpMultiplier;
    }

    protected override Vector3 ApplyOffset(float time, Vector3 inital)
    {
        return perpGetter.GetTangent(time) * mult + inital;
    }
}

// A wrapper for multiple path offsets applied in series
// Offsets are applied in the order they are provided.
public class CompositeOffsite : SkeletonPathOffset
{
    public override ISkeletonPath BasePath 
    { 
        // when changing base paths, switch out base path of the very first offset
        get => seriesStart.BasePath; 
        set => seriesStart.BasePath = value; 
    }

    SkeletonPathOffset seriesStart = null;
    SkeletonPathOffset seriesEnd = null;

    public SkeletonPathOffset FindComponent(System.Predicate<SkeletonPathOffset> IsMatch)
    {
        SkeletonPathOffset toCheck = seriesEnd as SkeletonPathOffset;
        while(toCheck != null)
        {
            if (IsMatch(toCheck))
                return toCheck;
            toCheck = toCheck.BasePath as SkeletonPathOffset;
        }
        return null;
    }

    public T FindComponent<T>() where T: SkeletonPathOffset
    {
        return FindComponent(
            (SkeletonPathOffset spo) => spo is T
        ) as T;
    }

    public CompositeOffsite(params SkeletonPathOffset[] components)
    {
        if (components.Length < 2)
            Debug.LogError("Composite offset needs at least two components to function correctly");

        seriesStart = components[0];
        seriesEnd = components[components.Length - 1];

        // apply offsets in the order they're provided
        for (int i = 1; i < components.Length; i++)
        {
            components[i].BasePath = components[i - 1];
        }
    }

    protected override Vector3 ApplyOffset(float time, Vector3 inital)
    {
        return seriesEnd.GetPointOnPath(time);
    }
}
