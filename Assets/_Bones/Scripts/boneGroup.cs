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
    protected Dictionary<BoneVertexType, Vector3> vertexPositions;

    protected Vector3 primaryAxis;
    protected Vector3 auxiliaryAxis;
    protected Vector3 startingUpDir;

    protected virtual void Awake()
    {
        InitVertexData();
        InitGeometryValues();
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void FixedUpdate()
    {
        Debug.DrawLine(vertexPositions[BoneVertexType.FrontPrimary], vertexPositions[BoneVertexType.FrontPrimary] + startingUpDir * 10, Color.blue);
        Debug.DrawLine(vertexPositions[BoneVertexType.BackPrimary], vertexPositions[BoneVertexType.BackPrimary] + startingUpDir * 10, Color.blue);
        Debug.DrawLine(vertexPositions[BoneVertexType.LeftAux], vertexPositions[BoneVertexType.LeftAux] + startingUpDir * 10, Color.blue);
        Debug.DrawLine(vertexPositions[BoneVertexType.RightAux], vertexPositions[BoneVertexType.RightAux] + startingUpDir * 10, Color.blue);
    }
    protected void InitVertexData()
    {
        children = new Dictionary<BoneVertexType, BoneGroup>();
        vertexPositions = new Dictionary<BoneVertexType, Vector3>();
        vertexPositions.Add(BoneVertexType.FrontPrimary, frontPrimaryVertex.transform.localPosition);
        vertexPositions.Add(BoneVertexType.BackPrimary, backPrimaryVertex.transform.localPosition);
        vertexPositions.Add(BoneVertexType.LeftAux, leftAuxVertex.transform.localPosition);
        vertexPositions.Add(BoneVertexType.RightAux, rightAuxVertex.transform.localPosition);

    }
    protected void InitGeometryValues()
    {
        primaryAxis = vertexPositions[BoneVertexType.BackPrimary] - vertexPositions[BoneVertexType.FrontPrimary];
        auxiliaryAxis = vertexPositions[BoneVertexType.LeftAux] - vertexPositions[BoneVertexType.RightAux];
        startingUpDir = Vector3.Cross(primaryAxis, auxiliaryAxis);
    }

    public bool isBeingDragged;
    public bool isLeaf;
    public bool isAttached;

    public void Attach(BoneGroup parent, TableManager tableManager)
    {
        this.parent = parent;
        isLeaf = true;
        parent.isLeaf = false;
        isAttached = true;
    }
  
}
