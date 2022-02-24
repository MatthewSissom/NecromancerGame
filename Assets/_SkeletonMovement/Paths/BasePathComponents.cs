﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//---Path component classes---//
interface IBasicPathComponent
{
    public abstract Vector3 GetPointNearPath(float time, float distanceFromPath, bool rightOfPath);
}

//moves target at a constant speed from start to end
public class LinePath : ISkeletonPath, IBasicPathComponent
{
    Vector3 start;
    Vector3 end;
    Vector3 forward;

    public LinePath(float duration, Vector3 start, Vector3 end)
    {
        Duration = duration;
        this.start = start;
        this.end = end;
        forward = (end - start);
        forward.y = 0;
        forward.Normalize();
    }

    public override Vector3 GetPointOnPath(float time, out Vector3 forward)
    {
        forward = this.forward;
        return Vector3.Lerp(start, end, time / Duration);
    }

    public override Vector3 GetPointOnPath(float time)
    {
        return Vector3.Lerp(start, end, time / Duration);
    }

    public Vector3 GetPointNearPath(float time, float distanceFromPath, bool rightOfPath)
    {
        Vector3 inital = Vector3.Lerp(start, end, time / Duration);
        //get a perpendicular normal vector and scale it by distance from path
        Vector3 offset = new Vector3(-forward.z, 0, forward.x)
            * distanceFromPath
            * (rightOfPath ? -1 : 1);

        return inital + offset;
    }
}

//moves target along a circle on the ground at a constant speed
public class SemicirclePath : ISkeletonPath, IBasicPathComponent
{
    Vector3 center;
    float rad;
    float startTheta;
    float deltaTheta;

    public SemicirclePath(float duration, Vector3 center, float rad, float startTheta, float endTheta)
    {
        Duration = duration;
        this.center = center;
        this.rad = rad;
        this.startTheta = startTheta;
        deltaTheta = endTheta - startTheta;
    }

    public override Vector3 GetPointOnPath(float time)
    {
        float theta = startTheta + deltaTheta * (time / Duration);
        return center + new Vector3(Mathf.Cos(theta) * rad, 0, Mathf.Sin(theta) * rad);
    }
    public override Vector3 GetPointOnPath(float time, out Vector3 forward)
    {
        float theta = startTheta + deltaTheta * (time / Duration);
        Vector3 fromCenterToPath = new Vector3(Mathf.Cos(theta) * rad, 0, Mathf.Sin(theta) * rad);
        forward = new Vector3(-fromCenterToPath.z, 0, fromCenterToPath.x) * Mathf.Sign(deltaTheta);
        return center + fromCenterToPath;
    }

    public Vector3 GetPointNearPath(float time, float distanceFromPath, bool rightOfPath)
    {
        float theta = startTheta + deltaTheta * (time / Duration);
        //either grow or shrink the rad depending on the orientation of the path (clockwise or not)
        //and if the point should be on the left or right side of it. XOR is used because it is the 
        //only logical opperator that the output flips if an input flips
        float newRad = rad + distanceFromPath * ((rightOfPath ^ deltaTheta < 0) ? 1 : -1);
        return center + new Vector3(Mathf.Cos(theta) * newRad, 0, Mathf.Sin(theta) * newRad);
    }
}

//moves a target along an arc following newtons laws
public class JumpArc : ISkeletonPath, IBasicPathComponent
{
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
        Duration = timeToApex + timeAfterApex;

        initYVel = -gravConst * timeToApex;
        delta.y = 0;
        constVelComponent = delta / Duration;
        constPerp = new Vector3(-constVelComponent.z, 0, constVelComponent.x).normalized;
    }

    public Vector3 GetPointNearPath(float time, float distanceFromPath, bool rightOfPath)
    {
        return GetPointOnPath(time) + constPerp * distanceFromPath * (rightOfPath ? -1 : 1);
    }

    public override Vector3 GetPointOnPath(float time)
    {
        return initPos + (constVelComponent * time) + new Vector3(0, initYVel * time + gravConst * time * time / 2, 0);
    }

    public override Vector3 GetPointOnPath(float time, out Vector3 forward)
    {
        forward = (constVelComponent + new Vector3(0, initYVel + time * gravConst, 0)).normalized;
        return initPos + constVelComponent * (time / Duration) + new Vector3(0, initYVel * time + gravConst * time * time / 2, 0);
    }
}