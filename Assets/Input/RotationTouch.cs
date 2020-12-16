using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTouch : TouchProxy
{
    private BoneMovingTouch parent;
    public BoneMovingTouch Parent
    {
        get { return parent; }
        set
        {
            parent = value;
            //replace this touch with a functionless one if the floating touch is removed
            parent.DestroyEvent += () => { InputManager.Instance.DisableTouch(this); };
        }
    }
    public float toRotate = 0;

    public override void Move(Vector3 pos, float rad)
    {
        Vector3 previousNorm = (parent.transform.position - transform.position).normalized;
        base.Move(pos, rad);
        toRotate += Vector3.SignedAngle(previousNorm, (parent.transform.position - transform.position).normalized, Vector3.up);
    }

    protected void Update()
    {
        if(parent.activeBone)
            parent.applyToAll((Bone toApply, FunctionArgs e) =>
            {
                toApply.transform.RotateAround(parent.activeBone.transform.root.position, Vector3.up, toRotate);
            });
        toRotate = 0;
    }
}
