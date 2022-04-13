using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeShoulderRoot : IAssemblyStage
{
    public IEnumerator Execute(GameObject skeleton, IAssemblyStage previous)
    {
        ResidualBoneData shoulderBone = null;
        ResidualBoneData startingRootBone = null;
        foreach (ResidualBoneData bone in TableManager.Instance.residualBoneData)
        {
            if (bone.isShoulder)
            {
                shoulderBone = bone;
            }
            if(bone.isRoot)
            {
                startingRootBone = bone;
            }
        }
        if(shoulderBone == startingRootBone)
        {
            yield break;
        }

        ResidualBoneData stepBone = shoulderBone;
        List<ResidualBoneData> swapList = new List<ResidualBoneData>();
        while(stepBone != startingRootBone)
        {
            swapList.Add(stepBone);
            stepBone = stepBone.parentBone;
        }

        swapList.Reverse();

        foreach (ResidualBoneData bone in swapList)
        {
            bone.SwapWithParent();
        }

        yield break;
    }
}
