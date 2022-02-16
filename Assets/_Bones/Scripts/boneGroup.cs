using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoneGroupType { Neck, Head, Ribcage, Spine, Pelvis, Tail, FrontLeftLeg, FrontLeftFoot, FrontRightLeg, FrontRightFoot, BackLeftLeg, BackLeftFoot, BackRightLeg, BackRightFoot}
public enum BoneVertexType { FrontPrimary, BackPrimary, LeftAux, RightAux}
public class BoneGroup : MonoBehaviour
{

    [SerializeField]
    protected GameObject frontPrimaryVertex;

    [SerializeField]
    protected GameObject backPrimaryVertex;

    [SerializeField]
    protected GameObject leftAuxVertex;

    [SerializeField]
    protected GameObject rightAuxVertex;


    protected BoneGroup parent;
    protected Dictionary<BoneVertexType, BoneGroup> children;

    protected virtual void Awake()
    {
        children = new Dictionary<BoneVertexType, BoneGroup>();
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void FixedUpdate()
    {
        Debug.DrawLine(getPrimaryMidpoint(), getPrimaryMidpoint() + getAuxiliaryAxis() * 0.5f, Color.blue);

        Debug.DrawLine(getVertexPosition(BoneVertexType.FrontPrimary), getVertexPosition(BoneVertexType.FrontPrimary) + getAuxiliaryAxis() * 0.5f, Color.yellow);
        Debug.DrawLine(getVertexPosition(BoneVertexType.BackPrimary), getVertexPosition(BoneVertexType.BackPrimary) + getAuxiliaryAxis() * 0.5f, Color.yellow);
        Debug.DrawLine(getVertexPosition(BoneVertexType.LeftAux), getVertexPosition(BoneVertexType.LeftAux) + getAuxiliaryAxis() * 0.5f, Color.yellow);
        Debug.DrawLine(getVertexPosition(BoneVertexType.RightAux), getVertexPosition(BoneVertexType.RightAux) + getAuxiliaryAxis() * 0.5f, Color.yellow);
    }

    public bool isBeingDragged;
    public bool isLeaf;
    public bool isAttached;
    //is right the actual forward vector
    public bool rightFoward;

    public Vector3 getVertexPosition(BoneVertexType type)
    {
        switch (type)
        {
            case BoneVertexType.FrontPrimary:
                return frontPrimaryVertex.transform.position;
            case BoneVertexType.BackPrimary:
                return backPrimaryVertex.transform.position;
            case BoneVertexType.LeftAux:
                return leftAuxVertex.transform.position;
            case BoneVertexType.RightAux:
                return rightAuxVertex.transform.position;
        }

        throw new Exception();
    }

    public Vector3 getPrimaryMidpoint()
    {
        return (getVertexPosition(BoneVertexType.FrontPrimary) + getVertexPosition(BoneVertexType.BackPrimary)) / 2;
    }

    public Vector3 getAuxiliaryAxis()
    {
        return (getVertexPosition(BoneVertexType.LeftAux) - getVertexPosition(BoneVertexType.RightAux)).normalized;
    }
    public void Attach(BoneGroup parent, TableManager tableManager)
    {
        this.parent = parent;
        isLeaf = true;
        parent.isLeaf = false;
        isAttached = true;
    }
  
}
