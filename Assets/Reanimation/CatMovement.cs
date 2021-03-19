using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Cat movement is in charge of the lowest level goals like moving a specific limb to a specific location
//Recives instrucitons from cat behavior
public class CatMovement
{
    GameObject mCat;

    float groundLevelY;
    float speed;

    CatPath path;

    //speed and ground height are temp, should be moved here eventually
    public CatMovement(List<LimbEnd> limbEnds, GameObject mCat, float groundHeight, float speed, CatPath path)
    {
        this.mCat = mCat;
        this.path = path;
        this.speed = speed;
        this.groundLevelY = groundHeight;

        foreach (var limb in limbEnds)
        {
            limb.StepStartEvent += LimbStartedStep;
            limb.StepEndEvent += LimbEndedStep;
        }
    }

    void LimbStartedStep(LimbEnd limb)
    {
        float time = (limb.StrideLength / 2 / speed) +  (limb.StrideLength / limb.StepSpeed);
        float dist = 0.05f;
        Vector3 target;

        //Calculate the new position for the limb
        switch (limb.LocationTag)
        {
            case LimbEnd.LimbLocationTag.FrontLeft:
                target = path.PointNearPath(time, dist, false);
                break;
            case LimbEnd.LimbLocationTag.FrontRight:
                target = path.PointNearPath(time, dist, true);
                break;
            case LimbEnd.LimbLocationTag.BackLeft:
                target = path.PointNearPath(time, dist, false, true);
                break;
            case LimbEnd.LimbLocationTag.BackRight:
                target = path.PointNearPath(time, dist, true,true);
                break;
            default:
                target = new Vector3();
                break;
        }

        target.y = groundLevelY;
        limb.SetStepTarget(target);
    }

    void LimbEndedStep(LimbEnd calling, Vector3? collisionPoint)
    {
        //if (collisionPoint != null)
        //{
        //    stablizerCount += 1;
        //    if (stablizerCount == maxStablizerCount)
        //    {
        //        StepWithNextLimb();
        //    }

        //    calling.StartPush();
        //}
        calling.StartPush();
    }
}
