using System.Collections;
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

    private Rigidbody bodyOfRigidness;
    private CustomGravity mCustomGravity;

    private bool firstPickup = true;

    Transform IGrabbable.transform { get { return transform; } }
    public Rigidbody Rb { get {  return bodyOfRigidness; } }
    public Vector3 PrimaryMidpoint { get { return getPrimaryMidpoint(); } }
    public Vector3 AuxilieryAxis { get { return getAuxiliaryAxis(); } }
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        //physics init
        bodyOfRigidness = GetComponent<Rigidbody>();
        ResetMass();
        mCustomGravity = GetComponent<CustomGravity>();
    }

    public void PickedUp()
    {
        if (mGhost)
            mGhost.LostBone();
      
        if (mCustomGravity)
            mCustomGravity.Disable();
        bodyOfRigidness.useGravity = false;
        
        if (RightForward)
        {
            if(firstPickup)
                transform.right = Camera.main.transform.forward * FlippedMultiplier;

            bodyOfRigidness.constraints = (RigidbodyConstraints) 96;
        }
        else
        {
            if(firstPickup)
                transform.forward = Camera.main.transform.forward * FlippedMultiplier;

            bodyOfRigidness.constraints = (RigidbodyConstraints)48;
        }

        OnPickup();
        firstPickup = false;
        IEnumerator DelayedLayerChange()
        {
            
            yield return new WaitForSeconds(0.4f);
            gameObject.layer = physicsLayer;
      
            yield break;
        }
        StartCoroutine(DelayedLayerChange());
        
    }

    public void Dropped()
    {
        
        if (!this)
            return;
        const float maxReleaseYVelocity = 1.0f;
        if (mCustomGravity)
            mCustomGravity.Enable();
       
        //rb.freezeRotation = false;
        gameObject.layer = physicsLayer;
        Vector3 velocity = bodyOfRigidness.velocity;
        if (Mathf.Abs(velocity.y) > maxReleaseYVelocity)
        {
            bodyOfRigidness.velocity = Vector3.ProjectOnPlane(velocity, Vector3.up) + (Vector3.up * maxReleaseYVelocity);
        }

        if(currentCylinderHit != null)
        {
            OnCollideDrop();
        } else
        {
            OnNoCollideDrop();
        }
        bodyOfRigidness.freezeRotation = true;
    }

    private void ResetMass()
    {
        bodyOfRigidness.SetDensity(density);
        //mass is considered temporary and will be written over unless directly set
        bodyOfRigidness.mass = bodyOfRigidness.mass;
    }
}
