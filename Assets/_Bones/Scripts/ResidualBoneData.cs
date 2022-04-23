using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ResidualBoneData : MonoBehaviour
{
    private Vector3 boneGroupPosition;
    public Dictionary<BoneVertexType, Vector3> vertexPositions;
    private Dictionary<BoneVertexType, ResidualBoneData> childBones;
    public ResidualBoneData parentBone;
    public BoneVertexType myParentConnectLocation;
    private BoneVertexType parentsMeConnectLocation;
    public bool isRoot;
    public bool isShoulderSideSpine;
    public bool isHipSideSpine;
    public bool isHead;
    public bool isNeck;
    public bool isTailStart;
    public bool isTail;
    public bool isShoulder;
    public bool isHip;
    public bool isFLLStart;
    public bool isFRLStart;
    public bool isBLLStart;
    public bool isBRLStart;
    public bool isFLLFoot;
    public bool isFRLFoot;
    public bool isBLLFoot;
    public bool isBRLFoot;
    public float myLegLength;
    public float numChildren;

    public bool isLegStart
    {
        get
        {
            return isFLLStart || isFRLStart || isBRLStart || isBLLStart;
        }
    }
    public void SwapWithParent()
    {
        childBones[myParentConnectLocation] = parentBone;
        parentBone.childBones[parentsMeConnectLocation] = null;
        parentBone.parentBone = this;
        parentBone.parentsMeConnectLocation = myParentConnectLocation;
        parentBone.myParentConnectLocation = parentsMeConnectLocation;
        if(parentBone.isRoot)
        {
            isRoot = true;
            parentBone.isRoot = false;
        }
        parentBone.transform.SetParent(transform);
    }
    public GameObject AddEmptyObjectBetweenSelfAndParent()
    {
        GameObject emptyObj = new GameObject(gameObject.name + " Joint");
        emptyObj.transform.position = vertexPositions[myParentConnectLocation];
        emptyObj.transform.parent = parentBone.transform;
        transform.parent = emptyObj.transform;
        return emptyObj;
    }
    public GameObject AddEmptyCapObject()
    {
        GameObject emptyObj = new GameObject(gameObject.name + " Cap");
        emptyObj.transform.position = vertexPositions[Opposite(myParentConnectLocation)];
        emptyObj.transform.parent = transform;
        return emptyObj;
    }
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
            myParentConnectLocation = boneGroup.myConnectionVertexType;
            parentsMeConnectLocation = boneGroup.Parent.FindBoneInChildren(boneGroup);
        }

    }

#if UNITY_EDITOR

    private Dictionary<BoneVertexType, ResidualBoneData> LazyInitChildBones()
    {
        // no init needed
        if (childBones != null)
            return childBones;

        childBones = new Dictionary<BoneVertexType, ResidualBoneData>();
        childBones[BoneVertexType.FrontPrimary] = null;
        childBones[BoneVertexType.BackPrimary] = null;
        childBones[BoneVertexType.LeftAux] = null;
        childBones[BoneVertexType.RightAux] = null;
        numChildren = 0;

        return childBones;
    }

    public Dictionary<BoneVertexType, Vector3> LazyInitVertexPositions()
    {
        //no init needed;
        if (vertexPositions != null)
            return vertexPositions;

        vertexPositions = new Dictionary<BoneVertexType, Vector3>();

        vertexPositions[BoneVertexType.FrontPrimary] = new Vector3(0,0,0);
        vertexPositions[BoneVertexType.BackPrimary] =  new Vector3(0,0,0);
        vertexPositions[BoneVertexType.LeftAux] =      new Vector3(0,0,0);
        vertexPositions[BoneVertexType.RightAux] = new Vector3(0, 0, 0);

        return vertexPositions;
    }

    public struct BoneConnectionData
    {
        public BoneVertexType vertexOnParent    ;
        public BoneVertexType vertexOnChild     ;
        public Vector3 position                 ;

        public BoneConnectionData(BoneVertexType vertexOnParent, BoneVertexType vertexOnChild, Vector3 position)
        {
            this.vertexOnParent = vertexOnParent;
            this.vertexOnChild = vertexOnChild;
            this.position = position;
        }
    }


    public void DebugIdInit(Vector3 position, bool isRoot = false)
    {
        boneGroupPosition = position;
        this.isRoot = isRoot;
    }

    public void DebugAddChild(ResidualBoneData child, BoneConnectionData connectionData)
    {
        childBones = LazyInitChildBones();
        if (child != null)
        {
            childBones[connectionData.vertexOnParent] = child;
            child.parentBone = this;
            child.parentsMeConnectLocation = connectionData.vertexOnParent;
            child.myParentConnectLocation = connectionData.vertexOnChild;
            child.LazyInitVertexPositions()[connectionData.vertexOnChild] = connectionData.position;
        }
        LazyInitVertexPositions()[connectionData.vertexOnParent] = connectionData.position;
        numChildren++;
    }

    public void AddCapConnectionPoint(BoneConnectionData connectionData)
    {
        LazyInitVertexPositions()[connectionData.vertexOnParent] = connectionData.position;
    }

#endif


    public void MarkShoulderSideSpine()
    {
#if UNITY_EDITOR
        if(DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.shoulderSpineMat;
            }
        }
#endif
        isShoulderSideSpine = true;
    }
    public void MarkHipSideSpine()
    {
#if UNITY_EDITOR
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.hipSpineMat;
            }
        }
#endif
        isHipSideSpine = true;
    }
    public void MarkHead()
    {
#if UNITY_EDITOR
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.headMat;
            }
        }
#endif
        isHead = true;
    }
    public void MarkTail()
    {
#if UNITY_EDITOR
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.tailMat;
            }
        }
#endif
        isTail = true;
    }
    public void MarkNeck()
    {
        isNeck = true;
    }

    public void MarkTailStart()
    {
        isTailStart = true;
    }
    public void MarkShoulder()
    {
#if UNITY_EDITOR
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.shoulderMat;
            }
        }
#endif
        isShoulder = true;
    }
    public void MarkHip()
    {
#if UNITY_EDITOR
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.hipMat;
            }
        }
#endif
        isHip = true;
    }
    public void MarkFLLStart()
    {
#if UNITY_EDITOR
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.leftLegStartMat;
            }
        }
#endif
        isFLLStart = true;
    }
    public void MarkFRLStart()
    {
#if UNITY_EDITOR
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.rightLegStartMat;
            }
        }
#endif
        isFRLStart = true;
    }
    public void MarkBLLStart()
    {
#if UNITY_EDITOR
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.leftLegStartMat;
            }
        }
#endif
        isBLLStart = true;
    }
    public void MarkBRLStart()
    {
#if UNITY_EDITOR
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.rightLegStartMat;
            }
        }
#endif
        isBRLStart = true;
    }
    public void MarkFLLFoot()
    {
#if UNITY_EDITOR
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.footMat;
            }
        }
#endif
        isFLLFoot = true;
    }
    public void MarkFRLFoot()
    {
#if UNITY_EDITOR
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.footMat;
            }
        }
#endif
        isFRLFoot = true;
    }
    public void MarkBLLFoot()
    {
#if UNITY_EDITOR
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.footMat;
            }
        }
#endif
        isBLLFoot = true;
    }
    public void MarkBRLFoot()
    {
#if UNITY_EDITOR
        if (DebugModes.ColorBonesInAssembly)
        {
            foreach (MeshRenderer r in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = CatPartHighlighters.instance.footMat;
            }
        }
#endif
        isBRLFoot = true;
    }
    public ResidualBoneData GetChild(BoneVertexType vertex)
    {
        Assert.IsNotNull(childBones);
        Assert.IsTrue(childBones.ContainsKey(vertex));
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

    public static BoneVertexType Opposite(BoneVertexType t)
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
