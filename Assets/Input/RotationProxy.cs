using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationProxy : TouchProxy
{
    private TouchProxy parent;
    public TouchProxy Parent
    { 
        get { return parent; }
        set 
        { 
            parent = value;
            previousNorm  = (parent.transform.position - transform.position).normalized;
        }
    }
    public Vector3 previousNorm;

    protected override void Update()
    {
        Vector3 norm = (parent.transform.position - transform.position).normalized;
        parent.RotateGroup(Vector3.SignedAngle(previousNorm, norm, Vector3.up));
        previousNorm = norm;
    }

    protected override void OnDestroy()
    {
    }
}
