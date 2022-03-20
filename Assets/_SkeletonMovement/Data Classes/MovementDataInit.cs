using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementDataInit
{
    BasicSpinePointData[] basicSpineData;

    float[] CalculateSpineHeights(LimbData[] limbEnds)
    {
        return new float[0];
        /*
        do
        {
            //how far off the ground the limb will be when walking
            minHeight = float.MaxValue;
            float avgHeight = 0;
            LimbData minLimb = null;
            foreach (var limb in limbEnds)
            {
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
            avgHeight /= limbEnds.Length;
            recalculate = minHeight < avgHeight / 2;
            if (recalculate)
                outliers.Add(minLimb);
        } while (recalculate);

        //distFromGroundToChest = minHeight;
        //ChestHeight = minHeight + GroundYValue;

        //foreach (var limb in limbEnds)
        //{
        //    limb.StepSpeed = speed * 4;
        //    limb.StepHeight = 0.05f;
        //    limb.SetStride(minHeight);
        //    limb.StepStartEvent += LimbStartedStep;
        //    limb.StepEndEvent += LimbEndedStep;
        //}
        */
    }
}
