using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationProxy : TouchProxy
{
    private FloatingTouch parent;
    public FloatingTouch Parent
    { 
        get { return parent; }
        set 
        { 
            parent = value;
            previousNorm  = (parent.transform.position - transform.position).normalized;
            //replace this touch with a functionless one
            parent.DestroyEvent += () => InputManager.Instance.ReplaceWith<TouchProxy>(this);
        }
    }
    public Vector3 previousNorm;

    protected void Update()
    {
        Vector3 norm = (parent.transform.position - transform.position).normalized;
        parent.applyToAll((bone toApply, FunctionArgs e) =>
        {
            float angle = Vector3.SignedAngle(previousNorm, norm, Vector3.up);
            toApply.transform.RotateAround(parent.transform.position + parent.offset, Vector3.up, angle);
        });
        previousNorm = norm;
    }
}
