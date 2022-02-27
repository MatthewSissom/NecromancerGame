using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Cat movement is in charge of the lowest level goals like moving a specific limb to a specific location
//Recives instrucitons from cat behavior
public class SkeletonMovement
{
    public float GroundYValue { get; private set; }
    public float ChestHeight { get; private set; }

    float distFromGroundToChest = 0;
    float speed;


    // if cat can't change paths immeditely (jumping) queue it instead
    public Vector3? queuedDestination = null;
    public event System.Action PathFinished;

    CompositePath path;
    bool pathing;

    //speed and ground height are temp, should be moved here eventually
    public SkeletonMovement(List<LimbEnd> limbEnds, float speed)
    {
        this.speed = speed;

        LimbInit(limbEnds);
    }

    public void SetPath(CompositePath basePath)
    {
        this.path = path;
        path.PathStarted += () => { pathing = true; };
        path.PathFinished += () => { pathing = false; };
        SkeletonPathfinding nav = path as SkeletonPathfinding;
        nav.GroundHeight = GroundYValue;
        nav.ChestHeightChange += (float val) => { SetGroundYValue(val- distFromGroundToChest); };
        foreach (var limb in limbEnds)
        {
            path.PathStarted += limb.PathStarted;
            path.JumpStarted += limb.StartJump;
        }
    }

    public void SetGroundYValue(float val)
    {
        GroundYValue = val;
        ChestHeight = val + distFromGroundToChest;
    }

    private void LimbInit(List<LimbEnd> limbEnds)
    {
        GroundYValue = 0;
        //holds limbs that shouldn't be considered for the min limb
        HashSet<LimbEnd> outliers = new HashSet<LimbEnd>();
        //if an outlier is found then remove it and check the set of limbs again
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

    private void ResetPath()
    {
        //add paths for each transfrom behind the shoulders to lerp to the position
        //of the key transform infront of it so at every point each key will be on the path
        Vector3 previousPos = default;
        var newPath = new LinkedList<PathComponent>();


        //no additional information, use linar paths for transforms behind the shoulders
        if (path == null || path.First == null)
        {
            //itterate backwards to go from head to tail
            for (int i = transforms.Length - 2; i >= 0; i--)
            {
                float delay = delays[i];
                if (delay > 0)
                {
                    newPath.AddFirst(new LinePath(delay - delays[i + 1],
                        transforms[i].position,
                        previousPos
                        ));
                }
                previousPos = transforms[i].position;
            }
            elapsedTime = delays[0];
        }
        //if the cat's target was changed while following another path use the old
        //path for higher precision
        else if (path.First.Value as SplitPath == null)
        {
            //itterate backwards to go from head to tail
            for (int i = transforms.Length - 2; i >= 0; i--)
            {
                float delay = delays[i];
                if (delay > 0)
                {
                    GetPointOnPath(-delay, out Vector3 mPos);
                    newPath.AddFirst(new LinePath(delay - delays[i + 1],
                        mPos,
                        previousPos
                        ));
                }
                GetPointOnPath(-delay, out previousPos);
            }
            elapsedTime = delays[0];
        }
        else
        {
            var split = path.First.Value as SplitPath;
            Vector3 pos;
            for (int i = transforms.Length - 2; i >= 0; i--)
            {
                split.SetIndex(i);
                pos = split.GetPointOnPath(split.SplitDuration);
                if (delays[i] > 0)
                {
                    newPath.AddFirst(new LinePath(delays[i] - delays[i + 1],
                        pos,
                        previousPos
                        ));
                }
                previousPos = pos;
            }
            elapsedTime = delays[0];
        }

        path = newPath;

        PathReset?.Invoke();
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
        if(pathing)
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Cats/Footsteps/SkeletonFootsteps");
        calling.StartPush();
    }
}
