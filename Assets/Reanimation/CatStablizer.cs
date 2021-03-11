using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatStablizer
{
    private Rigidbody rb;
    private float maxAcceleration;

    public CatStablizer(Rigidbody toStablize, float acceleration, float groundYVal)
    {
        rb = toStablize;
        maxAcceleration = acceleration;

        Vector3 centerOfMass = rb.worldCenterOfMass;
        centerOfMass.y = (groundYVal + centerOfMass.y)/2;
        //centerOfMass.y = -1;
        rb.centerOfMass = rb.transform.worldToLocalMatrix.MultiplyPoint(centerOfMass);
    }

    public delegate void DestablizedDelegate();
    public event DestablizedDelegate DestablizedEvent;

    public void Update(float deltaTime)
    {
        Vector3 axis = rb.transform.forward;
        float angleDistanceFromTarget = Vector3.SignedAngle(rb.transform.up, Vector3.Cross(axis,Vector3.right), axis);
        if (Mathf.Abs(angleDistanceFromTarget) > 30)
        {
            DestablizedEvent?.Invoke();
        }
        else if (Mathf.Abs(angleDistanceFromTarget) > 10)
        {

            float signedVelocityAroundAxis = Vector3.Dot(rb.angularVelocity, axis) * Mathf.Rad2Deg;
            float targetVelocity = Mathf.Sign(-angleDistanceFromTarget) * Mathf.Sqrt(2 * Mathf.Abs(angleDistanceFromTarget) * maxAcceleration);
            //calculate the correction force, clamped difference between current velocity and the target
            targetVelocity = signedVelocityAroundAxis - targetVelocity;
            if (Mathf.Abs(targetVelocity) > maxAcceleration)
                targetVelocity = Mathf.Sign(targetVelocity) * maxAcceleration;
            rb.angularVelocity += targetVelocity * axis * deltaTime;
        }
    }
}
