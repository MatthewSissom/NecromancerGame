﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableGroup : BoneGroup, IGrabbable
{
    //stores the ghost that is holding this bone
    public GhostBehavior mGhost;

    //physics
    //the layer bones should be placed on after being taken from a ghost
    const int physicsLayer = 10;
    //density of bone in kg/m^3
    const float realBoneDensity = 1850;
    const float density = realBoneDensity * 5;

    private CustomGravity mCustomGravity;
    private PointApproacher mPointApproacher;

    private bool firstPickup = true;
    public bool FirstPickup { get { return firstPickup; } }

    Transform IGrabbable.transform { get { return transform; } }
    public Rigidbody Rb { get { return rB; } }
    public Vector3 PrimaryMidpoint { get { return getPrimaryMidpoint(); } }
    public Vector3 AuxilieryAxis { get { return getAuxiliaryAxis(); } }

    [SerializeField]
    private float beingThrownThreshold;

    private float heightThreshold = 0;
    public float HeightSwitchOver { set { heightThreshold = value; } }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        //physics init
        
        ResetMass();
        mCustomGravity = GetComponent<CustomGravity>();
        mPointApproacher = GetComponent<PointApproacher>();
    }

    public void Thrown()
    { 

        rB.drag = 0.1f;
        DelayedLayerChange();

    }


    public void PickedUp()
    {
        if (mGhost)
            mGhost.LostBone();
      
        if (mCustomGravity)
            mCustomGravity.Disable();
        rB.useGravity = false;

        rB.drag = 0;

        if (firstPickup)
        {
            transform.forward = Camera.main.transform.forward * flippedMultiplier;
        }
        else
        {
            Rb.constraints = 0;
        }

        if (currentCylinderDoingHitting)
        {
            currentCylinderDoingHitting.SetConnectVisible();
        }
        if(currentCylinderHit)
        {
            currentCylinderHit.SetConnectVisible();
        }
        OnPickup();
        

    }

    public void Dropped()
    {
        
        if (!this)
            return;
        //Do not change velocity values if the bone has been thrown
        if(rB.velocity.magnitude > beingThrownThreshold)
        {
            if (mCustomGravity)
                mCustomGravity.Enable();
            rB.drag = 0.1f;
            OnNoCollideDrop();
        }
        else if(currentCylinderHit != null)
        {
           
            mPointApproacher.StartApproach(
                getRelativePosition(currentCollisionVertex.Value, currentCylinderHit.MyBone, currentCylinderHit.MyType), 0.5f);

            if (currentCylinderDoingHitting)
            {
                currentCylinderDoingHitting.SetConnectInvisible();
            }
            if (currentCylinderHit)
            {
                currentCylinderHit.SetConnectInvisible();
            }
            OnCollideDrop();
        } else
        {
            const float maxReleaseYVelocity = 1.0f;
            if (mCustomGravity)
                mCustomGravity.Enable();

            //rB.freezeRotation = false;
            gameObject.layer = physicsLayer;
            Vector3 velocity = rB.velocity;
            if (Mathf.Abs(velocity.y) > maxReleaseYVelocity)
            {
                rB.velocity = Vector3.ProjectOnPlane(velocity, Vector3.up) + (Vector3.up * maxReleaseYVelocity);
            }
            OnNoCollideDrop();
        }

        rB.freezeRotation = true;

        
        
    }

    private void ResetMass()
    {
        rB.SetDensity(density);
        //mass is considered temporary and will be written over unless directly set
        rB.mass = rB.mass;
    }
    public void DelayedLayerChange()
    {
        FullLayerChange(gameObject, physicsLayer);
        Rb.constraints = (RigidbodyConstraints)0;
        firstPickup = false;
    }
    
}
