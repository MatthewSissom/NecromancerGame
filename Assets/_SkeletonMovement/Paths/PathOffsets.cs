using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class SkeletonPathOffset : ISkeletonPath
{
    virtual public ISkeletonPath BasePath { get; set; } = default;
    public float Duration { get { return BasePath.Duration; } }

    public Vector3 GetPointOnPath(float time)
    {
        return ApplyOffset(time, BasePath.GetPointOnPath(time));
    }

    public static SkeletonPathOffset CombineOffsets(ISkeletonPath basePath, params SkeletonPathOffset[] offsets)
    {
        if (offsets.Length == 0)
            return null;

        offsets[0].BasePath = basePath;
        for(int i = 1; i < offsets.Length; i++)
        {
            offsets[i].BasePath = offsets[i - 1];
        }
        return offsets[offsets.Length - 1]; 
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
