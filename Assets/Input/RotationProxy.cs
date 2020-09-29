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
            parent.DestroyEvent += () => InputManager.Instance.replaceWithNull(this);
        }
    }
    public Vector3 previousNorm;

    protected override void Update()
    {
        Vector3 norm = (parent.transform.position - transform.position).normalized;
        parent.applyToAll((bone toApply, FunctionArgs e) =>
        {
            float angle = Vector3.SignedAngle(previousNorm, norm, Vector3.up);
            toApply.transform.RotateAround(parent.transform.position + parent.offset, Vector3.up, angle);
        });
        previousNorm = norm;
    }

    protected override void OnTriggerEnter(Collider other) { }
}
