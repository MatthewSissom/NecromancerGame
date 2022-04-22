using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalcResidualDataForPrefab : CalcResidualData
{
    protected override ResidualBoneData GetRootAndInitRBD(GameObject skeleton)
    {
#if UNITY_EDITOR

        Transform skull = skeleton.transform.GetChild(0);
        Transform skullEndMarker = skeleton.transform.GetChild(1);

        DFRecurse( skull, 
            (Transform parent, Transform child, int childIndex) => {
                var rbd = child.gameObject.AddComponent<ResidualBoneData>();
                TableManager.Instance.residualBoneData.Add(rbd);
                rbd.DebugIdInit(child.position, parent == null);
            }
        );
        DFRecurse(skull,
            (Transform parent, Transform child, int childIndex) => {
                if(parent != null)
                {
                    var pRBD = parent.GetComponent<ResidualBoneData>();
                    var cRBD = child.GetComponent<ResidualBoneData>();
                    if (pRBD != null)
                        pRBD.DebugAddChild(cRBD, GetConnectionData(child, childIndex));
                }
            }
        );

        return skull.GetComponent<ResidualBoneData>();
#endif
    }

    protected ResidualBoneData.BoneConnectionData GetConnectionData(Transform child, int childIndex)
    {
        switch(childIndex)
        {
            // no parent
            case -1:
                return new ResidualBoneData.BoneConnectionData(
                    BoneVertexType.FrontPrimary,
                    BoneVertexType.FrontPrimary,
                    child.position
                    );
            // primary connection
            case 0:
                return new ResidualBoneData.BoneConnectionData(
                    BoneVertexType.BackPrimary,
                    BoneVertexType.FrontPrimary,
                    child.position
                    );
            // left leg
            case 1:
                return new ResidualBoneData.BoneConnectionData(
                    BoneVertexType.LeftAux,
                    BoneVertexType.FrontPrimary,
                    child.position
                    );
            // right leg
            case 2:
                return new ResidualBoneData.BoneConnectionData(
                    BoneVertexType.RightAux,
                    BoneVertexType.FrontPrimary,
                    child.position
                    );
            default:
                throw new System.ArgumentOutOfRangeException("Child index not handled, check that test cat is set up propertly");
        }
    }

    protected void DFRecurse(Transform start, System.Action<Transform, Transform, int> applyToAll) 
    {
        for(int i = 0; i < start.childCount; ++i)
        {
            Transform child = start.GetChild(i);
            if(child.name != "Model")
                DFRecurse(child , applyToAll);
        }
        applyToAll(null, start, 0);
    }
}
