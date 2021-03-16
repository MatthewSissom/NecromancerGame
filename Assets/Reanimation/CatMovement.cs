using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Cat movement is in charge of the lowest level goals like moving a specific limb to a specific location
//Recives instrucitons from cat behavior
public class CatMovement
{
    GameObject mCat;

    Vector3 shoulderDirection;
    bool rotating;
    Vector3 hipDirection;

    float groundLevelY;

    CatPath path;

    public CatMovement(List<LimbEnd> limbEnds, GameObject mCat, float groundHeight, CatPath path)
    {
        this.mCat = mCat;
        this.path = path;

        foreach (var limb in limbEnds)
        {
            limb.StepStartEvent += LimbStartedStep;
            limb.StepEndEvent += LimbEndedStep;
        }
    }

    void LimbStartedStep(LimbEnd limb)
    {
        //Calculate the new position for the limb
        
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
