using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeShoulderRoot : IAssemblyStage
{
    public IEnumerator Execute(GameObject skeleton, IAssemblyStage previous)
    {
        ResidualBoneData shoulderBone;
        foreach (ResidualBoneData bone in TableManager.Instance.residualBoneData)
        {
            if (bone.isShoulder)
            {
                shoulderBone = bone;
            }
        }


        yield break;
    }
}
