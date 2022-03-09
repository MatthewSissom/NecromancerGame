using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Cat movement is in charge of the low level goals like coordinating limb movment
//Recives a base path from cat behavior
public class SkeletonMovement
{
    public bool Pathing { get; private set; }
    public event System.Action PathFinished;
    IContinuousSkeletonPath path;

    List<PathTracer> footTracers;
    List<PathTracer> spineTracers;

    //speed and ground height are temp, should be moved here eventually
    public SkeletonMovement(List<LimbData> limbEnds, List<SpineTransform>)
    {
        LimbInit(limbEnds);
    }

    public void SetPath(IContinuousSkeletonPath basePath)
    {
        this.path = basePath;
        foreach(var tracer in footTracers) 
        {
            tracer.Path = basePath;
        }
        foreach (var tracer in spineTracers)
        {
            tracer.Path = basePath;
        }
    }

    private void LimbInit(List<LimbData> limbEnds)
    {
        float GroundYValue = 0;
        //holds limbs that shouldn't be considered for the min limb
        HashSet<LimbData> outliers = new HashSet<LimbData>();
        //if an outlier is found then remove it and check the set of limbs again
        bool recalculate = false;
        float minHeight;

        do
        {
            //how far off the ground the limb will be when walking
            minHeight = float.MaxValue;
            float avgHeight = 0;
            LimbData minLimb = null;
            foreach (var limb in limbEnds)
            {
                if (outliers.Contains(limb))
                    continue;
                float length = limb.LimbLength;
                //reduce the lenght of limbs to account for bending
                //.7 ~> root(2)/2 which give the cat a stride lenght roughly twice the lenght of the limb
                //which is roughly realistic. stumps don't use this calculation because they can't bend
                if (limb.Type != LimbData.LimbTag.Stump && limb.Type != LimbData.LimbTag.StumpSingle)
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

#if UNITY_EDITOR
    private void RenderDebugPaths()
    {
        float simulatedTime = 0;
        const float simulatedTimeStep = .05f;
        List<Vector3> points = new List<Vector3>();
        // sample points on path for debugging
        DebugRendering.UpdatePath(DebugModes.DebugPathFlags.TruePath, points);
    }
#endif
}
