using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResidualBoneData : MonoBehaviour
{
    private Vector3 boneGroupPosition;
    private Dictionary<BoneVertexType, Vector3> vertexPositions;
    private Dictionary<BoneVertexType, ResidualBoneData> childBones;
    public ResidualBoneData parentBone;
    public BoneVertexType myParentConnectLocation;
    private BoneVertexType parentsMeConnectLocation;
    private bool isRoot;
    public bool isShoulderSideSpine;
    public bool isHipSideSpine;
    private bool isHead;
    private bool isTail;
    public bool isShoulder;
    private bool isHip;
    public bool isFLLStart;
    public bool isFRLStart;
    public bool isBLLStart;
    public bool isBRLStart;
    public float myLegLength;
    public float numChildren;

    public void PopulateDataFrom(GrabbableGroup boneGroup)
    {
        boneGroupPosition = boneGroup.transform.position;
        isRoot = boneGroup.isRoot;


        vertexPositions = new Dictionary<BoneVertexType, Vector3>();

        vertexPositions[BoneVertexType.FrontPrimary] = 
            boneGroup.getVertexPosition(BoneVertexType.FrontPrimary);
        vertexPositions[BoneVertexType.BackPrimary] = 
            boneGroup.getVertexPosition(BoneVertexType.BackPrimary);
        vertexPositions[BoneVertexType.LeftAux] = 
            boneGroup.getVertexPosition(BoneVertexType.LeftAux);
        vertexPositions[BoneVertexType.RightAux] = 
            boneGroup.getVertexPosition(BoneVertexType.RightAux);
    }

    public void PopulateChildrenFrom(GrabbableGroup boneGroup)
    {
        childBones = new Dictionary<BoneVertexType, ResidualBoneData>();
        numChildren = 0;
        if(boneGroup.children.ContainsKey(BoneVertexType.FrontPrimary) && boneGroup.children[BoneVertexType.FrontPrimary])
        {
            numChildren++;
            childBones[BoneVertexType.FrontPrimary] = boneGroup.children[BoneVertexType.FrontPrimary].residualBoneData;
        } else
        {
            childBones[BoneVertexType.FrontPrimary] = null;
        }
        if (boneGroup.children.ContainsKey(BoneVertexType.BackPrimary) && boneGroup.children[BoneVertexType.BackPrimary])
        {
            numChildren++;
            childBones[BoneVertexType.BackPrimary] = boneGroup.children[BoneVertexType.BackPrimary].residualBoneData;
        }
        else
        {
            childBones[BoneVertexType.BackPrimary] = null;
        }

        if (boneGroup.children.ContainsKey(BoneVertexType.LeftAux) && boneGroup.children[BoneVertexType.LeftAux])
        {
            numChildren++;
            childBones[BoneVertexType.LeftAux] = boneGroup.children[BoneVertexType.LeftAux].residualBoneData;
        }
        else
        {
            childBones[BoneVertexType.LeftAux] = null;
        }

        if (boneGroup.children.ContainsKey(BoneVertexType.RightAux) && boneGroup.children[BoneVertexType.RightAux])
        {
            numChildren++;
            childBones[BoneVertexType.RightAux] = boneGroup.children[BoneVertexType.RightAux].residualBoneData;
        }
        else
        {
            childBones[BoneVertexType.RightAux] = null;
        }

        if (boneGroup.Parent)
        {
            parentBone = boneGroup.Parent.residualBoneData;
            myParentConnectLocation = boneGroup.parentVertexCollisionType;
            parentsMeConnectLocation = boneGroup.Parent.FindBoneInChildren(boneGroup);
        }

    }
    public void MarkShoulderSideSpine()
    {
        if(DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.shoulderSpineMat;
            }
        }
        isShoulderSideSpine = true;
    }
    public void MarkHipSideSpine()
    {
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.hipSpineMat;
            }
        }
        isHipSideSpine = true;
    }
    public void MarkHead()
    {
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.headMat;
            }
        }
        isHead = true;
    }
    public void MarkTail()
    {
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.tailMat;
            }
        }
        isTail = true;
    }
    public void MarkShoulder()
    {
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.shoulderMat;
            }
        }
        isShoulder = true;
    }
    public void MarkHip()
    {
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.hipMat;
            }
        }
        isHip = true;
    }
    public void MarkFLLStart()
    {
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.leftLegStartMat;
            }
        }
        isFLLStart = true;
    }
    public void MarkFRLStart()
    {
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.rightLegStartMat;
            }
        }
        isFRLStart = true;
    }
    public void MarkBLLStart()
    {
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.leftLegStartMat;
            }
        }
        isBLLStart = true;
    }
    public void MarkBRLStart()
    {
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.rightLegStartMat;
            }
        }
        isBRLStart = true;
    }
    public void MarkFoot()
    {
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.footMat;
            }
        }
    }
    public ResidualBoneData GetChild(BoneVertexType vertex)
    {
        return childBones[vertex];
    }
    private float VDistance(BoneVertexType v1, BoneVertexType v2)
    {
        return Vector3.Distance(vertexPositions[v1], vertexPositions[v2]);
    }
    private float getMaxAcrossDistance(BoneVertexType startVertex)
    {
        return Mathf.Max(
            VDistance(startVertex, BoneVertexType.FrontPrimary),
            VDistance(startVertex, BoneVertexType.BackPrimary),
            VDistance(startVertex, BoneVertexType.LeftAux),
            VDistance(startVertex, BoneVertexType.RightAux)
        );
    }

    private static BoneVertexType Opposite(BoneVertexType t)
    {
        switch (t)
        {
            case BoneVertexType.FrontPrimary:
                return BoneVertexType.BackPrimary;
            case BoneVertexType.BackPrimary:
                return BoneVertexType.FrontPrimary;
            case BoneVertexType.LeftAux:
                return BoneVertexType.RightAux;
            case BoneVertexType.RightAux:
                return BoneVertexType.LeftAux;
        }
        return BoneVertexType.FrontPrimary;
    }

    public static BoneVertexType Left(BoneVertexType t)
    {
        switch (t)
        {
            case BoneVertexType.FrontPrimary:
                return BoneVertexType.RightAux;
            case BoneVertexType.BackPrimary:
                return BoneVertexType.LeftAux;
            case BoneVertexType.LeftAux:
                return BoneVertexType.FrontPrimary;
            case BoneVertexType.RightAux:
                return BoneVertexType.BackPrimary;
        }
        return BoneVertexType.FrontPrimary;
    }

    public static BoneVertexType Right(BoneVertexType t)
    {
        switch (t)
        {
            case BoneVertexType.FrontPrimary:
                return BoneVertexType.LeftAux;
            case BoneVertexType.BackPrimary:
                return BoneVertexType.RightAux;
            case BoneVertexType.LeftAux:
                return BoneVertexType.BackPrimary;
            case BoneVertexType.RightAux:
                return BoneVertexType.FrontPrimary;
        }
        return BoneVertexType.FrontPrimary;
    }


    public BoneVertexType? getAcrossVertex()
    {
        if(numChildren == 0)
        {
            return null;
        }
        if(childBones[Opposite(myParentConnectLocation)])
        {
            return Opposite(myParentConnectLocation);
        }
        if (childBones[BoneVertexType.FrontPrimary])
        {
            return BoneVertexType.FrontPrimary;
        }
        if (childBones[BoneVertexType.BackPrimary])
        {
            return BoneVertexType.BackPrimary;
        }
        if (childBones[BoneVertexType.LeftAux])
        {
            return BoneVertexType.LeftAux;
        }
        if (childBones[BoneVertexType.RightAux])
        {
            return BoneVertexType.RightAux;
        }
        return null;
    }

    public float distanceToRootBone(BoneVertexType? lastBonesConnectionVertex)
    {
        if(isRoot)
        {
            return 0;
        } else
        {
            if(lastBonesConnectionVertex == null)
            {
                return getMaxAcrossDistance(myParentConnectLocation) + parentBone.distanceToRootBone(parentsMeConnectLocation);
            } else
            {
                return VDistance(lastBonesConnectionVertex.Value, myParentConnectLocation) + parentBone.distanceToRootBone(parentsMeConnectLocation);
            }
        }
    }

    public float distanceToParentedBone(BoneVertexType? lastBonesConnectionVertex, Predicate<ResidualBoneData> predicate)
    {
        if (predicate(this))
        {
            return 0;
        }
        else
        {
            if (lastBonesConnectionVertex == null)
            {
                return getMaxAcrossDistance(myParentConnectLocation) + parentBone.distanceToParentedBone(parentsMeConnectLocation, predicate);
            }
            else
            {
                return VDistance(lastBonesConnectionVertex.Value, myParentConnectLocation) + parentBone.distanceToParentedBone(parentsMeConnectLocation, predicate);
            }
        }
    }
}
