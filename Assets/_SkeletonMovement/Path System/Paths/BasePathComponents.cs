using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//moves target at a constant speed from start to end
public class LinePath : IContinuousSkeletonPath
{
    public float EndTime { get; private set; }
    Vector3 start;
    Vector3 end;
    Vector3 forward;

    public LinePath(float duration, Vector3 start, Vector3 end)
    {
        EndTime = duration;
        this.start = start;
        this.end = end;
        forward = (end - start);
        forward.y = 0;
        forward.Normalize();
    }

    public Vector3 GetPointOnPath(float time)
    {
        return Vector3.Lerp(start, end, time / EndTime);
    }

    Vector3 IContinuousPath.GetTangent(float time)
    {
        return new Vector3(-forward.z, 0, forward.x);
    }

    Vector3 IContinuousPath.GetForward(float time)
    {
        return forward;
    }
}

//moves target along a circle on the ground at a constant speed
public class SemicirclePath : IContinuousSkeletonPath
{
    public float EndTime { get; private set; }
    Vector3 center;
    float rad;
    float startTheta;
    float deltaTheta;

    public SemicirclePath(float duration, Vector3 center, float rad, float startTheta, float endTheta)
    {
        EndTime = duration;
        this.center = center;
        this.rad = rad;
        this.startTheta = startTheta;
        deltaTheta = endTheta - startTheta;
    }

    public  Vector3 GetPointOnPath(float time)
    {
        float theta = startTheta + deltaTheta * (time / EndTime);
        return center + new Vector3(Mathf.Cos(theta) * rad, 0, Mathf.Sin(theta) * rad);
    }

    Vector3 IContinuousPath.GetTangent(float time)
    {
        float theta = startTheta + deltaTheta * (time / EndTime);
        return new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * -Mathf.Sign(deltaTheta);
    }

    Vector3 IContinuousPath.GetForward(float time)
    {
        float theta = startTheta + deltaTheta * (time / EndTime);
        return new Vector3(-1 * Mathf.Sin(theta), 0, Mathf.Cos(theta)) * Mathf.Sign(deltaTheta);
    }
}

//moves a target along an arc following newtons laws
public class JumpArc : IContinuousSkeletonPath
{
    public float EndTime { get; private set; }
    const float gravConst = -4;

    Vector3 initPos;
    Vector3 constVelComponent;
    Vector3 constPerp;
    public float initYVel { get; private set; }

    //apexHeight is measured from the start position
    public JumpArc(Vector3 start, Vector3 end, float apexHeight)
    {
        initPos = start;
        Vector3 delta = end - start;

        float timeToApex = Mathf.Sqrt(2 * apexHeight / (-1 * gravConst));
        float timeAfterApex = Mathf.Sqrt(Mathf.Abs(2 * (apexHeight - delta.y) / gravConst));
        EndTime = timeToApex + timeAfterApex;

        initYVel = -gravConst * timeToApex;
        delta.y = 0;
        constVelComponent = delta / EndTime;
        constPerp = new Vector3(-constVelComponent.z, 0, constVelComponent.x).normalized;
    }

    public Vector3 GetPointOnPath(float time)
    {
        return initPos + (constVelComponent * time) + new Vector3(0, initYVel * time + gravConst * time * time / 2, 0);
    }

    Vector3 IContinuousPath.GetTangent(float time)
    {
        return constPerp;
    }

    Vector3 IContinuousPath.GetForward(float time)
    {
        return (constVelComponent + new Vector3(0, initYVel + time * gravConst, 0)).normalized;
    }
}