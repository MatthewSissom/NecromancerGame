using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsertIntoArmature : IAssemblyStage
{
    public IEnumerator Execute(GameObject skeleton, IAssemblyStage previous)
    {
        GameObject armature = TableManager.Instance.EmptyArmature;
        EmptyArmatureData armatureData = armature.GetComponent<EmptyArmatureData>();

        ResidualBoneData head = null;
        ResidualBoneData shoulder = null;
        ResidualBoneData hip = null;
        ResidualBoneData tail = null;
        ResidualBoneData fll = null;
        ResidualBoneData frl = null;
        ResidualBoneData bll = null;
        ResidualBoneData brl = null;
        foreach (ResidualBoneData bone in TableManager.Instance.residualBoneData)
        {
            if (bone.isHead)
            {
                head = bone;
            }
            if (bone.isShoulder)
            {
                shoulder = bone;
            }
            if (bone.isHip)
            {
                hip = bone;
            }
            if (bone.isTail)
            {
                tail = bone;
            }
            if (bone.isFLLStart)
            {
                fll = bone;
            }
            if (bone.isFRLStart)
            {
                frl = bone;
            }
            if (bone.isBLLStart)
            {
                bll = bone;
            }
            if (bone.isBRLStart)
            {
                brl = bone;
            }
        }

        //here's where i'm stuck - the CalcResidualData stage is an IikTransformProvider and has working transforms in its SkeletonTransforms,
        //but it's a couple steps in the past so i can't just look at previous here. I could either make all the intermediate steps also IikTransformProviders
        //and just pass the SkeletonTransforms through, or, rebuild a new SkeletonTransforms by finding all the transforms like above, but both feel like the wrong approach
        yield break;
    }
}
