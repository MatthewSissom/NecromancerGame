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

    [SerializeField]
    private GameObject cylColliderPrefab;
    [SerializeField]
    private GameObject vertexIndicatorPrefab;

    protected Collider frontPrimaryCollider;
    protected Collider backPrimaryCollider;
    protected Collider leftAuxCollider;
    protected Collider rightAuxCollider;

    private GameObject frontPrimaryCylinder;
    private GameObject backPrimaryCylinder;
    private GameObject leftAuxCylinder;
    private GameObject rightAuxCylinder;

    private GameObject frontPrimaryIndicator;
    private GameObject backPrimaryIndicator;
    private GameObject leftAuxIndicator;
    private GameObject rightAuxIndicator;


    private float cylinderSize;

    protected BoneGroup parent;
    protected BoneVertexType parentVertexCollisionType;
    protected Dictionary<BoneVertexType, BoneGroup> children;

    [SerializeField] //for debugging
    public BoneCollisionCylinder currentCylinderHit;

    protected virtual void Awake()
    {
        children = new Dictionary<BoneVertexType, BoneGroup>();
        frontPrimaryCylinder = Instantiate(cylColliderPrefab, transform); 
        frontPrimaryIndicator = Instantiate(vertexIndicatorPrefab, frontPrimaryVertex.transform.position, Quaternion.identity, transform);
        InitCylinder(frontPrimaryCylinder, frontPrimaryIndicator, BoneVertexType.FrontPrimary);
        
        backPrimaryCylinder = Instantiate(cylColliderPrefab, transform); 
        backPrimaryIndicator = Instantiate(vertexIndicatorPrefab, backPrimaryVertex.transform.position, Quaternion.identity, transform);
        InitCylinder(backPrimaryCylinder, backPrimaryIndicator, BoneVertexType.BackPrimary); 
        

        leftAuxCylinder = Instantiate(cylColliderPrefab, transform);
        leftAuxIndicator = Instantiate(vertexIndicatorPrefab, leftAuxVertex.transform.position, Quaternion.identity, transform);
        InitCylinder(leftAuxCylinder, leftAuxIndicator, BoneVertexType.LeftAux);


        rightAuxCylinder = Instantiate(cylColliderPrefab, transform);
        rightAuxIndicator = Instantiate(vertexIndicatorPrefab, rightAuxVertex.transform.position, Quaternion.identity, transform);
        InitCylinder(rightAuxCylinder, rightAuxIndicator, BoneVertexType.RightAux);

        frontPrimaryCollider = frontPrimaryCylinder.GetComponent<Collider>();
        backPrimaryCollider = backPrimaryCylinder.GetComponent<Collider>();
        leftAuxCollider = leftAuxCylinder.GetComponent<Collider>();
        rightAuxCollider = rightAuxCylinder.GetComponent<Collider>();

        cylinderSize = frontPrimaryCylinder.transform.lossyScale.y;

        if(isAttached)
        {
            //transform.right = Vector3.right;
            //transform.forward = Vector3.Cross(Vector3.right, Camera.main.transform.forward * -1);
            //transform.RotateAround(tra)
        }
        else 
        {
            frontPrimaryCylinder.SetActive(false);
            backPrimaryCylinder.SetActive(false);
            leftAuxCylinder.SetActive(false);
            rightAuxCylinder.SetActive(false);
        }

    }

    protected virtual void Start()
    {
        
    }

    protected virtual void FixedUpdate()
    {
        if(isRoot)
        {
            transform.forward = Camera.main.transform.forward;
        }
        Debug.DrawLine(getPrimaryMidpoint(), getPrimaryMidpoint() + getAuxiliaryAxis() * 0.5f, Color.blue);

        Debug.DrawLine(getVertexPosition(BoneVertexType.FrontPrimary), getVertexPosition(BoneVertexType.FrontPrimary) + getAuxiliaryAxis() * 0.5f, Color.yellow);
        Debug.DrawLine(getVertexPosition(BoneVertexType.BackPrimary), getVertexPosition(BoneVertexType.BackPrimary) + getAuxiliaryAxis() * 0.5f, Color.yellow);
        Debug.DrawLine(getVertexPosition(BoneVertexType.LeftAux), getVertexPosition(BoneVertexType.LeftAux) + getAuxiliaryAxis() * 0.5f, Color.yellow);
        Debug.DrawLine(getVertexPosition(BoneVertexType.RightAux), getVertexPosition(BoneVertexType.RightAux) + getAuxiliaryAxis() * 0.5f, Color.yellow);

        PositionCylinder(frontPrimaryCylinder, frontPrimaryVertex);
        PositionCylinder(backPrimaryCylinder, backPrimaryVertex);
        PositionCylinder(leftAuxCylinder, leftAuxVertex);
        PositionCylinder(rightAuxCylinder, rightAuxVertex);
    }

    public bool isBeingDragged;
    public bool isLeaf;
    public bool isAttached;
    public bool isRoot;
    //is right the actual forward vector
    public bool rightFoward;
    //Does this bone need to be flipped by default to fit the canvas of the cat? Should be -1 or 1 but kept an int to be easily worked into our code
    public int flippedMuliplier;

    private GameObject getVertex(BoneVertexType type)
    {
        switch (type)
        {
            case BoneVertexType.FrontPrimary:
                return frontPrimaryVertex;
            case BoneVertexType.BackPrimary:
                return backPrimaryVertex;
            case BoneVertexType.LeftAux:
                return leftAuxVertex;
            case BoneVertexType.RightAux:
                return rightAuxVertex;
        }
        throw new Exception();
    }

    public Vector3 getVertexPosition(BoneVertexType type)
    {
        return getVertex(type).transform.position;
    }

    public Vector3 getPrimaryMidpoint()
    {
        return (getVertexPosition(BoneVertexType.FrontPrimary) + getVertexPosition(BoneVertexType.BackPrimary)) / 2;
    }

    public Vector3 getAuxiliaryAxis()
    {
        return (getVertexPosition(BoneVertexType.LeftAux) - getVertexPosition(BoneVertexType.RightAux)).normalized;
    }

    private void PositionCylinder(GameObject cylinder, GameObject vertex)
    {
        cylinder.transform.position = vertex.transform.position;// + getAuxiliaryAxis() * cylinderSize;
        cylinder.transform.up = getAuxiliaryAxis() * flippedMuliplier;
    }

    private void InitCylinder(GameObject cylinder, GameObject indicator, BoneVertexType type)
    {
        BoneCollisionCylinder colCyl = cylinder.GetComponent<BoneCollisionCylinder>();

        colCyl.MyBone = this;
        colCyl.MyType = type;
        colCyl.MyVertex = getVertex(type);
        colCyl.MyIndicator = indicator;
    }

    public void Attach(BoneGroup parent/*, TableManager tableManager*/)
    {
        Debug.Log("Attaching to: ");
        Debug.Log(parent);
        this.parent = parent;
        isLeaf = true;
        parent.isLeaf = false;
        isAttached = true;
    }

    protected void OnPickup()
    {
        isBeingDragged = true;

        frontPrimaryCylinder.SetActive(true);
        backPrimaryCylinder.SetActive(true);
        leftAuxCylinder.SetActive(true);
        rightAuxCylinder.SetActive(true);
    }

    protected void OnNoCollideDrop()
    {
        isBeingDragged = false;

        frontPrimaryCylinder.SetActive(false);
        backPrimaryCylinder.SetActive(false);
        leftAuxCylinder.SetActive(false);
        rightAuxCylinder.SetActive(false);
    }

    protected void OnCollideDrop()
    {
        BoneGroup otherGroup = currentCylinderHit.MyBone;

        isBeingDragged = false;

        frontPrimaryCylinder.SetActive(true);
        backPrimaryCylinder.SetActive(true);
        leftAuxCylinder.SetActive(true);
        rightAuxCylinder.SetActive(true);

        Attach(otherGroup);
    }
}
