using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoneGroupType { Neck, Head, Ribcage, Spine, Pelvis, Tail, FrontLeftLeg, FrontLeftFoot, FrontRightLeg, FrontRightFoot, BackLeftLeg, BackLeftFoot, BackRightLeg, BackRightFoot}
public enum BoneVertexType { FrontPrimary, BackPrimary, LeftAux, RightAux}
public class BoneGroup : MonoBehaviour
{
    private bool isCleaned;

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


    public ResidualBoneData residualBoneData;

    private float cylinderSize;

    protected BoneGroup parent;
    public BoneGroup Parent
    {
        get { return parent; }
    }
    /// <summary>
    /// The vertex of the parent that this bone is connected to
    /// </summary>
    public BoneVertexType parentVertexCollisionType;
    public Dictionary<BoneVertexType, BoneGroup> children;

    [SerializeField] //for debugging
    public BoneCollisionCylinder currentCylinderHit;

    public BoneCollisionCylinder currentCylinderDoingHitting;

    public BoneVertexType? currentCollisionVertex;

    // Tutorial vars
    public bool IsOnFloor { get; private set; } = false;

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

        residualBoneData = gameObject.AddComponent<ResidualBoneData>();
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void FixedUpdate()
    {
        if(!isCleaned)
        {
            if (isRoot)
            {
                transform.forward = Camera.main.transform.forward;
            }

            //Debug.DrawLine(getPrimaryMidpoint(), getPrimaryMidpoint() + getAuxiliaryAxis() * 0.5f, Color.blue);

            //Debug.DrawLine(getVertexPosition(BoneVertexType.FrontPrimary), getVertexPosition(BoneVertexType.FrontPrimary) + getAuxiliaryAxis() * 0.5f, Color.yellow);
            //Debug.DrawLine(getVertexPosition(BoneVertexType.BackPrimary), getVertexPosition(BoneVertexType.BackPrimary) + getAuxiliaryAxis() * 0.5f, Color.yellow);
            //Debug.DrawLine(getVertexPosition(BoneVertexType.LeftAux), getVertexPosition(BoneVertexType.LeftAux) + getAuxiliaryAxis() * 0.5f, Color.yellow);
            //Debug.DrawLine(getVertexPosition(BoneVertexType.RightAux), getVertexPosition(BoneVertexType.RightAux) + getAuxiliaryAxis() * 0.5f, Color.yellow);

            AlignAllCylindersToCamera();
        }
    }

    public bool isBeingDragged;
    public bool isLeaf;
    public bool isAttached;
    public bool isRoot;
    [SerializeField]
    protected int flippedMultiplier;
    //Does this bone need to be flipped by default to fit the canvas of the cat? Should be -1 or 1 but kept an int to be easily worked into our code
    public int FlippedMultiplier { get { return flippedMultiplier; } }

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

        // Matthew changed this, I think it feels better but it's up for debate
        Vector3 toCamera = Camera.main.transform.position - transform.position;
        cylinder.transform.up = toCamera * FlippedMultiplier;
    }

    public void AlignAllCylindersToCamera()
    {
        PositionCylinder(frontPrimaryCylinder, frontPrimaryVertex);
        PositionCylinder(backPrimaryCylinder, backPrimaryVertex);
        PositionCylinder(leftAuxCylinder, leftAuxVertex);
        PositionCylinder(rightAuxCylinder, rightAuxVertex);
    }

    private void InitCylinder(GameObject cylinder, GameObject indicator, BoneVertexType type)
    {
        BoneCollisionCylinder colCyl = cylinder.GetComponent<BoneCollisionCylinder>();

        colCyl.MyBone = this;
        colCyl.MyType = type;
        colCyl.MyVertex = getVertex(type);
        colCyl.MyIndicator = indicator;
    }

    public void Attach(BoneGroup parent)
    {
#if UNITY_EDITOR
        // I'm sick of debug logs! I'm hiding them! I don't wanna see em!
        if (DebugModes.AdditionalAssemblerInfo)
            Debug.Log("attaching to parent's " + parentVertexCollisionType);
#endif
        parent.children[parentVertexCollisionType] = this;
        this.parent = parent;
        isLeaf = true;
        parent.isLeaf = false;
        isAttached = true;
    }

    protected void OnPickup()
    {
        isBeingDragged = true;
        IsOnFloor = false;

        frontPrimaryCylinder.SetActive(true);
        backPrimaryCylinder.SetActive(true);
        leftAuxCylinder.SetActive(true);
        rightAuxCylinder.SetActive(true);

        if(parent)
        {
            parent.children[parentVertexCollisionType] = null;
            parent.isLeaf = true;
        }
        parent = null;
        isLeaf = false;
        isAttached = false;
        currentCollisionVertex = null;
        if(currentCylinderHit)
        {
            currentCylinderHit.IsParentsActiveCollision = false;
            currentCylinderHit = null;
        }
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
        parentVertexCollisionType = currentCylinderHit.MyType;

        isBeingDragged = false;

        frontPrimaryCylinder.SetActive(true);
        backPrimaryCylinder.SetActive(true);
        leftAuxCylinder.SetActive(true);
        rightAuxCylinder.SetActive(true);

        Attach(otherGroup);
    }

    public Vector3 getRelativePosition(BoneVertexType myVertexType, BoneGroup otherBone, BoneVertexType otherVertexType)
    {
        return otherBone.getVertexPosition(otherVertexType) + transform.position - getVertexPosition(myVertexType);
    }

    public void CleanUpIndicators()
    {
        Destroy(frontPrimaryCylinder);
        Destroy(backPrimaryCylinder);
        Destroy(leftAuxCylinder);
        Destroy(rightAuxCylinder);

        Destroy(frontPrimaryIndicator);
        Destroy(backPrimaryIndicator);
        Destroy(leftAuxIndicator);
        Destroy(rightAuxIndicator);

        Destroy(frontPrimaryVertex);
        Destroy(backPrimaryVertex);
        Destroy(leftAuxVertex);
        Destroy(rightAuxVertex);

        isCleaned = true;
    }
    public BoneVertexType FindBoneInChildren(BoneGroup bone)
    {
        if (children.ContainsKey(BoneVertexType.FrontPrimary) && children[BoneVertexType.FrontPrimary] == bone)
        {
            return BoneVertexType.FrontPrimary;
        }
        if (children.ContainsKey(BoneVertexType.BackPrimary) && children[BoneVertexType.BackPrimary] == bone)
        {
            return BoneVertexType.BackPrimary;
        }
        if (children.ContainsKey(BoneVertexType.LeftAux) && children[BoneVertexType.LeftAux] == bone)
        {
            return BoneVertexType.LeftAux;
        }
        if (children.ContainsKey(BoneVertexType.RightAux) && children[BoneVertexType.RightAux] == bone)
        {
            return BoneVertexType.RightAux;
        }
        return BoneVertexType.FrontPrimary;
    }

    private void OnCollisionEnter(Collision collision)
    {
        IsOnFloor = collision.gameObject.CompareTag("Horizontal") && collision.gameObject.name == "Floor";
    }
}
