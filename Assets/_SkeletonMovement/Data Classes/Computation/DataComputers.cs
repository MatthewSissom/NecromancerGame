using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinePointHeight : ValComputer<float>
{
    public ValComputer<float>[] LimbLenghts { private get; set; } = null;

    protected override float ComputeVal()
    {
        float maxHeight = 0;
        float avgBentHeight = 0;
        for (int i = 0; i < LimbLenghts.Length; i++)
        {
            float length = LimbLenghts[i];

            //reduce the lenght of limbs to account for bending
            //.7 ~> root(2)/2 which give the cat a stride lenght roughly twice the lenght of the limb
            //which is roughly realistic. stumps don't use this calculation because they can't bend
            float comfortableBend = length * .7f;
            float minBend = length * .9f;

            // lower max so all limbs can move
            if (minBend < maxHeight)
                maxHeight = minBend;

            avgBentHeight += comfortableBend;
        }
        avgBentHeight /= LimbLenghts.Length;

        base.ComputeVal();
        return Mathf.Min(avgBentHeight, maxHeight);
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

public class LimbIdData : ValComputer<LimbIdentityData>
{
    public Transform LimbStart { private get; set; }

    protected override LimbIdentityData ComputeVal()
    {
        //TODO
        LimbIdentityData data = new LimbIdentityData(true, true, true, true);

        base.ComputeVal();
        return data;
    }
}