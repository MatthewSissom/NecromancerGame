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
}
