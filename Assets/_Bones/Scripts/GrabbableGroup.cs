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
    private Rigidbody rb;
    private Dictionary<Collider, Bone> colliderToBone;
    BoneCollisionHandler collisionHandler;

    Transform IGrabbable.transform { get { return transform; } }
    public Rigidbody Rb { get { return rb; } }
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        colliderToBone = new Dictionary<Collider, Bone>();

        void RecursiveColliderSearch(Transform toCheck, Bone bone = null)
        {
            if (!bone)
            {
                if(TryGetComponent(out bone))
                    bone.AttachedEvent += BoneWasConnected;
            }
            else
            {
                if(TryGetComponent(out Collider c))
                    colliderToBone.Add(c,bone);
            }

            for (int i = 0; i < toCheck.childCount; i++)
                RecursiveColliderSearch(toCheck.GetChild(i), bone);
        }
        RecursiveColliderSearch(transform);
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
        //TEMP cash custom gravity
        var gravity = GetComponent<CustomGravity>();
        if(gravity)
            gravity.enabled = true;
        rb.freezeRotation = false;
        Vector3 velocity = rb.velocity;
        if (Mathf.Abs(velocity.y) > maxReleaseYVelocity)
        {
            rb.velocity = Vector3.ProjectOnPlane(velocity, Vector3.up) + (Vector3.up * maxReleaseYVelocity);
        }
    }

    public void BoneWasConnected(Bone bone)
    {
        bone.transform.parent = null;
        RemoveChild(bone.Group);
    }

    public Bone BoneFromCollider(Collider collider)
    {
        if (colliderToBone.TryGetValue(collider, out Bone toReturn))
            return toReturn;
        return null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        colliderToBone.TryGetValue(collision.GetContact(0).thisCollider, out Bone collided);
        if(collided)
            collisionHandler.AddBoneCollision(collided, collision);
        //---Bone on horizontal surface---//
        if (collision.gameObject.CompareTag("Horizontal"))
        {
            Rb.velocity = new Vector3(0, Rb.velocity.y, 0);
            //TEMP cash custom gravity 
            Rb.gameObject.GetComponent<CustomGravity>().enabled = false;
            Rb.useGravity = true;
        }
    }
}
