using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Cat movement is in charge of the lowest level goals like moving a specific limb to a specific location
//Recives instrucitons from cat behavior
public class CatMovement
{
    public float GroundYValue { get; private set; }
    public float ChestHeight { get; private set; }

    float distFromGroundToChest = 0;
    float speed;

    CatPath path;
    bool pathing;

    //speed and ground height are temp, should be moved here eventually
    public CatMovement(List<LimbEnd> limbEnds, float speed)
    {
        this.speed = speed;

        LimbInit(limbEnds);
    }

    public void SetPath(CatPath path, List<LimbEnd> limbEnds)
    {
        this.path = path;
        path.PathStarted += () => { pathing = true; };
        path.PathFinished += () => { pathing = false; };
        (path as CatPathWithNav).GroundHeight = GroundYValue;

        foreach(var limb in limbEnds)
            path.PathStarted += limb.PathStarted;
    }

    public void SetGroundYValue(float val)
    {
        GroundYValue = val;
        ChestHeight = val + distFromGroundToChest;
    }

    //temp move to movement
    private void LimbInit(List<LimbEnd> limbEnds)
    {
        GroundYValue = 0;
        //holds limbs that shouldn't be considered for the min limb
        HashSet<LimbEnd> outliers = new HashSet<LimbEnd>();
        //if an outlier is found then 
        bool recalculate = false;
        float minHeight;

        do
        {
            //how far off the ground the limb will be when walking
            minHeight = float.MaxValue;
            float avgHeight = 0;
            LimbEnd minLimb = null;
            foreach (var limb in limbEnds)
            {
                if (outliers.Contains(limb))
                    continue;
                float length = limb.LimbLength;
                //reduce the lenght of limbs to account for bending
                //.7 ~> root(2)/2 which give the cat a stride lenght roughly twice the lenght of the limb
                //which is roughly realistic. stumps don't use this calculation because they can't bend
                if (limb.Type != LimbEnd.LimbTag.Stump && limb.Type != LimbEnd.LimbTag.StumpSingle)
                    length *= .7f;
                if (length < minHeight)
                {
                    minHeight = length;
                    minLimb = limb;
                }
                avgHeight += length;
            }
            avgHeight /= limbEnds.Count;
            recalculate = minHeight < avgHeight / 2;
            if (recalculate)
                outliers.Add(minLimb);
        } while (recalculate);

        distFromGroundToChest = minHeight;
        ChestHeight = minHeight + GroundYValue;

        foreach (var limb in limbEnds)
        {
            limb.StepSpeed = speed * 4;
            limb.StepHeight = 0.05f;
            limb.SetStride(minHeight);
            limb.StepStartEvent += LimbStartedStep;
            limb.StepEndEvent += LimbEndedStep;
        }
    }

    void LimbStartedStep(LimbEnd limb)
    { 
        float time = (limb.StrideLength / speed) +  (limb.StrideLength / limb.StepSpeed);
        //move directly under the cat if it's standing still
        time *= pathing ? 1 : 0;
        float dist = 0.05f;
        Vector3 target;

        if(!path.IsValid)
        {
            limb.DefaultStepTarget();
            return;
        }

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
        if (float.IsNaN(target.x) || float.IsNaN(target.y) || float.IsNaN(target.z))
            Debug.Log("Invalid position from path");

        target.y = GroundYValue;
        limb.SetStepTarget(target, (limb.StrideLength / speed));
    }

    void LimbEndedStep(LimbEnd calling, Vector3? collisionPoint)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Cats/Footsteps/SkeletonFootsteps");
        calling.StartPush();
    }
}
