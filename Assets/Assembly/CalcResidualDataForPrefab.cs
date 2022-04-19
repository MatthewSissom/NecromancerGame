using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalcResidualDataForPrefab : CalcResidualData
{
    protected override ResidualBoneData GetRootAndInitRBD(GameObject skeleton)
    {
#if UNITY_EDITOR

        Transform root = skeleton.transform.GetChild(0);




        //creating and populating residual data with initial position/parent-child relations
        foreach (GrabbableGroup bone in TableManager.Instance.boneObjects)
        {
            bone.GetComponent<ResidualBoneData>().PopulateDataFrom(bone);
            bone.GetComponent<ResidualBoneData>().PopulateChildrenFrom(bone);
        }

        ResidualBoneData rootBoneData = null;
        foreach (GrabbableGroup bone in TableManager.Instance.boneObjects)
        {
            TableManager.Instance.residualBoneData.Add(bone.residualBoneData);
            if (bone.isRoot)
            {
                rootBoneData = bone.residualBoneData;
            }
        }
        return rootBoneData;
#endif
    }
}
