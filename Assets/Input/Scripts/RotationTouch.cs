using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTouch : TouchProxy
{
    public float rotAngleAroundUp = 0;
    public float rotAngleAroundToParent = 0;
    public Vector3 toParent;
    public float toParentMagnitude;

    const float aroundToParentMult = 1000;
    private bool aroundUp = true;

    private BoneMovingTouch parent;
    public BoneMovingTouch Parent
    {
        get { return parent; }
        set
        {
            parent = value;
            parent.RemoveAngularDragMult();
            //replace this touch with a functionless one if the floating touch is removed
            parent.DisableEvent += () => { InputManager.Instance.DisableTouch(this); };
        }
    }

    public void ResetTouch(Vector3 pos, float rad)
    {
        base.Move(pos, rad);
        toParent = parent.transform.position - transform.position;
        toParentMagnitude = toParent.magnitude;
        toParent /= toParentMagnitude; //normalize

        rotAngleAroundUp = 0;
        rotAngleAroundToParent = 0;
    }

    public override void Move(Vector3 pos, float rad)
    {        
        //store old values
        Vector3 oldToParent = toParent;
        rotAngleAroundToParent = toParentMagnitude;

        //update position
        base.Move(pos, rad);
        toParent = parent.transform.position - transform.position;
        toParentMagnitude = toParent.magnitude;
        toParent /= toParentMagnitude; //normalize

        //calculate angles
        rotAngleAroundUp += Vector3.SignedAngle(oldToParent, toParent, Vector3.up);
        //calculate difference between old (stored in rotAngleAroudToParent) and new then
        //multiply to get the angle to rotate
        rotAngleAroundToParent = (rotAngleAroundToParent-toParentMagnitude) * aroundToParentMult;
    }

    protected void Update()
    {
        if (!parent.activeBone)
            return;

        if (aroundUp && Mathf.Abs(rotAngleAroundToParent) > Mathf.Abs(rotAngleAroundUp * 5))
        {
            parent.SetAxisOfRotation(Vector3.Cross(Vector3.up,toParent));
            aroundUp = false;
        }
        if (!aroundUp && Mathf.Abs(rotAngleAroundUp) > Mathf.Abs(rotAngleAroundToParent * 5))
        {
            parent.SetAxisOfRotation(Vector3.up);
            aroundUp = true;
        }

        parent.angularVelocity += (aroundUp? rotAngleAroundUp: rotAngleAroundToParent)*2;

        rotAngleAroundToParent = 0;
        rotAngleAroundUp = 0;
    }

    protected void OnDisable()
    {
        if(parent)
            parent.ApplyAngularDragMult();
    }
}
