using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Cat movement is in charge of the lowest level goals like moving a specific limb to a specific location
//Recives instrucitons from cat behavior
public class CatMovement
{
    float groundYVal;
    float stepHeight;
    float speed;

    CatPath path;
    bool pathing;

    //speed and ground height are temp, should be moved here eventually
    public CatMovement(List<LimbEnd> limbEnds, float stepHeight, float speed, CatPath path)
    {
        this.stepHeight = stepHeight;
        this.path = path;
        path.PathStarted += () => { pathing = true; };
        path.PathFinished += () => { pathing = false; };

        this.speed = speed;

        LimbInit(limbEnds);
        (path as CatPathWithNav).GroundHeight = groundYVal;
    }

    //temp move to movement
    private void LimbInit(List<LimbEnd> limbEnds)
    {
        groundYVal = 0;
        foreach (var limb in limbEnds)
        {
            groundYVal += limb.transform.position.y;
        }
        groundYVal /= limbEnds.Count;

        foreach (var limb in limbEnds)
        {
            limb.StepSpeed = speed * 4;
            limb.StepHeight = stepHeight;
            limb.StepStartEvent += LimbStartedStep;
            limb.StepEndEvent += LimbEndedStep;
            limb.TempLimbInit(groundYVal);

            //void RecursiveColliderSearch(Transform toCheck)
            //{
            //    if (toCheck.TryGetComponent(out Collider c))

            //        for (int i = 0; i < toCheck.childCount; i++)
            //            RecursiveColliderSearch(toCheck.GetChild(i));
            //}
            //RecursiveColliderSearch(limb.transform);
        }
    }

    void LimbStartedStep(LimbEnd limb)
    { 
        float time = (limb.LimbLength*3 / 2 / speed) +  (limb.LimbLength*3 / limb.StepSpeed);
        time *= pathing ? 1 : 0;
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

        target.y = groundYVal;
        limb.SetStepTarget(target);
    }

    void LimbEndedStep(LimbEnd calling, Vector3? collisionPoint)
    {
        calling.StartPush();
    }
}
