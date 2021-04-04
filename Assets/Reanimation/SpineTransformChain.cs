using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineTransformChain : TransformChain
{
    List<Transform> ribTransforms;
    public SpineTransformChain(Transform[] transforms, Transform[] ribTransforms, GameObject target, bool hasOffset) : base (transforms,target,hasOffset)
    {
        this.ribTransforms = new List<Transform>(ribTransforms);
    }

    public override bool Contains(Transform toCheck)
    {
        return base.Contains(toCheck) || ribTransforms.Contains(toCheck);
    }

    public override Transform MoveToFirstEmpty(Transform originalDestination)
    {
        if(!ribTransforms.Contains(originalDestination))
        {
            return base.MoveToFirstEmpty(originalDestination);
        }
        else
        {
            if(ribTransforms.IndexOf(originalDestination) == firstEmpty)
            {
                return FirstEmpty();
            }
        }
        return originalDestination;
    }
}
