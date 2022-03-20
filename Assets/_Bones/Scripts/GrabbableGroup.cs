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

    private Rigidbody rB;
    private CustomGravity mCustomGravity;
    private PointApproacher mPointApproacher;

    private bool firstPickup = true;

    Transform IGrabbable.transform { get { return transform; } }
    public Rigidbody Rb { get {  return rB; } }
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
        rB = GetComponent<Rigidbody>();
        ResetMass();
        mCustomGravity = GetComponent<CustomGravity>();
        mPointApproacher = GetComponent<PointApproacher>();
    }

    public void PickedUp()
    {
        if (mGhost)
            mGhost.LostBone();
      
        if (mCustomGravity)
            mCustomGravity.Disable();
        rB.useGravity = false;
        
        
        if(firstPickup)
          transform.forward = Camera.main.transform.forward * flippedMultiplier;

        rB.constraints = (RigidbodyConstraints)48;
       

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

        if(currentCylinderHit != null)
        {
            mPointApproacher.StartApproach(
                getRelativePosition(currentCollisionVertex.Value, currentCylinderHit.MyBone, currentCylinderHit.MyType), 0.5f);

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
        IEnumerator DelayedRotationLock()
        {

            yield return new WaitForSeconds(0.2f);
            rB.freezeRotation = true;
            yield break;
        }
        StartCoroutine(DelayedRotationLock());
        
    }

    private void ResetMass()
    {
        rB.SetDensity(density);
        //mass is considered temporary and will be written over unless directly set
        rB.mass = rB.mass;
    }
}
