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
    const float density = 1850;

    private Rigidbody rb;
    //maps colliders to the bones they belong to
    private Dictionary<Collider, Bone> colliderToBone;
    BoneCollisionHandler collisionHandler;
    private CustomGravity mCustomGravity;

    Transform IGrabbable.transform { get { return transform; } }
    public Rigidbody Rb { get { return rb; } }
    protected override void Awake()
    {
        base.Awake();

        //physics init
        rb = GetComponent<Rigidbody>();
        ResetMass();
        colliderToBone = new Dictionary<Collider, Bone>();
        mCustomGravity = GetComponent<CustomGravity>();

        //finds all bones in the higharcy and adds their colliders to the dictionary
        //returns if a bone was found lower in the higharchy
        bool RecursiveColliderSearch(Transform toCheck, Bone bone = null)
        {
            bool boneFound = false;
            if (!bone)
            {
                //search for bone if none has been found 
                boneFound = toCheck.TryGetComponent(out bone);
                if (boneFound)
                    bone.AttachedEvent += BoneWasConnected;
            }
            else
            {
                //if bones have been found search for colliders which belong to 
                //the found bone
                if(toCheck.TryGetComponent(out Collider c))
                    colliderToBone.Add(c,bone);
            }

            //recurse
            for (int i = 0; i < toCheck.childCount; i++)
                boneFound |= RecursiveColliderSearch(toCheck.GetChild(i), bone);

            return boneFound;
        }
        bool isValid = RecursiveColliderSearch(transform);
        if (!isValid)
            Debug.LogError("Compound bone has no children!");
    }

    protected override void Start()
    {
        base.Start();
        collisionHandler = BoneManager.Collision;
    }

    public void PickedUp()
    {
        if (mGhost)
            mGhost.LostBone();

        rb.freezeRotation = false;
        rb.useGravity = false;

        IEnumerator DelayedLayerChange()
        {
            yield return new WaitForSeconds(0.2f);
            gameObject.layer = physicsLayer;
            ApplyToAll((Bone b, FunctionArgs args) =>
            {
                BoneManager.Collision.SetPhysicsLayer(b, physicsLayer);
            });
            yield break;
        }
        StartCoroutine(DelayedLayerChange());
    }

    public void Dropped()
    {
        if (!this)
            return;
        const float maxReleaseYVelocity = 1.0f;
        if(mCustomGravity)
            mCustomGravity.enabled = true;
        rb.freezeRotation = false;
        Vector3 velocity = rb.velocity;
        if (Mathf.Abs(velocity.y) > maxReleaseYVelocity)
        {
            rb.velocity = Vector3.ProjectOnPlane(velocity, Vector3.up) + (Vector3.up * maxReleaseYVelocity);
        }
    }

    protected override void RemoveChild(BoneGroup toRemove)
    {
        base.RemoveChild(toRemove);
        if (children.Count == 0)
            Destroy(gameObject);
    }

    public void BoneWasConnected(Bone bone)
    {
        bone.transform.parent = null;
        RemoveChild(bone.Group);
        ResetMass();
    }

    public Bone BoneFromCollider(Collider collider)
    {
        if (colliderToBone.TryGetValue(collider, out Bone toReturn))
            return toReturn;
        return null;
    }

    private void ResetMass()
    {
        Rb.SetDensity(density);
        //mass is considered temporary and will be written over unless directly set
        Rb.mass = Rb.mass;
    }

    private void OnCollisionEnter(Collision collision)
    {
        colliderToBone.TryGetValue(collision.GetContact(0).thisCollider, out Bone collided);
        if(collided)
            collisionHandler.AddBoneCollision(collided, collision);
        //---Bone on horizontal surface---//
        if (collision.gameObject.CompareTag("Horizontal") && mCustomGravity.enabled)
        {
            if (mCustomGravity)
                mCustomGravity.Disable();
        }
    }
}
