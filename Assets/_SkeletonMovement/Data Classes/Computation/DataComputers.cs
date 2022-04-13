using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinePointPositions : ValComputer<LabeledSpineData<Vector2>>
{
    public LabeledLimbData<LimbLength> LimbLenghts { private get; set; } = null;
    public LabeledSpineData<SpineLenght> SpineLengths { private get; set; } = null;

    private float GetHeightFromLimbs(float left, float right, out float maxHeight)
    {
        maxHeight = float.MaxValue;
        float avgBentHeight = 0;
        var limbArr = LimbLenghts.ToList();

        if(left == 0 && right == 0)
        {
            maxHeight = 0;
            return 0;
        }

        //reduce the lenght of limbs to account for bending
        //.7 ~> root(2)/2 which give the cat a stride lenght roughly twice the lenght of the limb
        //which is roughly realistic. stumps don't use this calculation because they can't bend
        if(left != 0)
        { 
            float comfortableBend = left * .7f;
            avgBentHeight += comfortableBend;
            float minBend = left * .9f;
            // lower max so all limbs can move
            if (minBend < maxHeight)
                maxHeight = minBend;
        }
        if (right != 0)
        {
            float comfortableBend = right * .7f;
            avgBentHeight += comfortableBend;
            float minBend = right * .9f;
            // lower max so all limbs can move
            if (minBend < maxHeight)
                maxHeight = minBend;
        }
        avgBentHeight /= (left != 0 && right != 0) ? 2 : 1;
        return avgBentHeight;
    }

    protected override LabeledSpineData<Vector2> ComputeVal()
    {
        float hipBestHeight = GetHeightFromLimbs(LimbLenghts.BackLeftLeg, LimbLenghts.BackRightLeg, out float hipMaxHeight);
        float shoulderBestHeight = GetHeightFromLimbs(LimbLenghts.FrontLeftLeg, LimbLenghts.FrontRightLeg, out float shoulderMaxHeight);
        float spineDist = SpineLengths.Hip;

        float shoulderHeight = shoulderBestHeight;
        float hipHeight = hipBestHeight;
        float signedDist = hipHeight - shoulderHeight;
        // bring vals in bounds of Asin
        while (Mathf.Abs(signedDist) > spineDist)
        {
            if(signedDist < 0)
            {
                shoulderHeight *= .9f;
                hipHeight *= 1.1f;
                hipHeight = Mathf.Min(hipHeight, hipMaxHeight);
            }
            else
            {
                shoulderHeight *= 1.1f;
                hipHeight *= 0.9f;
                shoulderHeight = Mathf.Min(shoulderHeight, shoulderMaxHeight);
            }
            signedDist = hipHeight - shoulderHeight;
        }

        // angle between hips and shoulders should be < 30 degrees
        float signedAngle = Mathf.Asin(hipHeight - shoulderHeight / spineDist);
        while (Mathf.Abs(signedAngle) > Mathf.PI / 6)
        {
            if (signedAngle < 0)
            {
                shoulderHeight *= .9f;
                hipHeight *= 1.1f;
                hipHeight = Mathf.Min(hipHeight, hipMaxHeight);
            }
            else
            {
                shoulderHeight *= 1.1f;
                hipHeight *= 0.9f;
                shoulderHeight = Mathf.Min(shoulderHeight, shoulderMaxHeight);
            }
            signedAngle = Mathf.Asin(hipHeight - shoulderHeight / spineDist);
        }

        const float headAngle = Mathf.PI / 8;
        float headHeight = Mathf.Sin(headAngle) * SpineLengths.Head + shoulderHeight;
        const float tailAngle = Mathf.PI / 4; 

        Vector2 headPos     = new Vector2(0, headHeight);
        Vector2 shoulderPos = new Vector2(headPos.x + SpineLengths.Head * Mathf.Cos(headAngle), shoulderHeight);
        Vector2 hipPos      = new Vector2(shoulderPos.x + SpineLengths.Hip * Mathf.Cos(signedAngle), hipHeight);
        Vector2 tailPos = new Vector2(hipPos.x + SpineLengths.Tail * Mathf.Cos(tailAngle), hipHeight + Mathf.Sin(tailAngle));

        base.ComputeVal();
        return new LabeledSpineData<Vector2>(
            headPos,
            shoulderPos,
            hipPos,
            tailPos
            );
    }
}

public class LimbLength : ValComputer<float>
{
    public Transform LimbStart { private get; set; }

    protected override float ComputeVal()
    {
        float totalLen = 0;


        base.ComputeVal();
        return totalLen;
    }
}

public class SpineLenght : ValComputer<float>
{
    public Transform From { private get; set; }
    public Transform To { private get; set; }

    protected override float ComputeVal()
    {
        float totalLen = 0;


        base.ComputeVal();
        return totalLen;
    }
}


public class StrideLenght : ValComputer<float>
{
    public ValComputer<float> SpinePointHeight { private get; set; }
    public ValComputer<float> LimbLength { private get; set; }

    protected override float ComputeVal()
    {
        base.ComputeVal();

        //avoid irrational and 0 solutions
        if (LimbLength <= SpinePointHeight)
            return LimbLength;
        else
            return Mathf.Sqrt(LimbLength * LimbLength - SpinePointHeight * SpinePointHeight);
    }
}

public class StepHeight : ValComputer<float>
{
    public ValComputer<float> LimbLength { private get; set; }
    public ValComputer<float> StrideLenght { private get; set; }

    protected override float ComputeVal()
    {
        base.ComputeVal();
        return Mathf.Min(StrideLenght / 3, .2f * LimbLength);
    }
}

public class LimbOffset : ValComputer<float>
{
    public LabeledLimbData<LimbLength> AllLimbLenghts { private get; set; }
    protected override float ComputeVal()
    {
        float totalLenght = 0;
        int limbCnt = 0;
        foreach(float length in AllLimbLenghts.ToList())
        {
            totalLenght += length;
            if (length != 0)
                limbCnt++;
        }
        totalLenght /= limbCnt;

        base.ComputeVal();
        return totalLenght * .2f;
    }
}


public class LimbIdData : ValComputer<LimbIdentityData>
{
    public Transform LimbStart { private get; set; }

    protected override LimbIdentityData ComputeVal()
    {
        //TODO
        LimbIdentityData data = new LimbIdentityData(new OpenLimbIdentityData());

        base.ComputeVal();
        return data;
    }
}