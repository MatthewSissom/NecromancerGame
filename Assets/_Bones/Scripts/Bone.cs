using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour, IGrabbable
{
    //physics
    const int physicsLayer = 10;
    protected Rigidbody rb;
    protected static BoneCollisionHandler collisionHandler;

    //bone group, used to store relationships
    //between bones
    protected BoneGroup group;
    //stores the ghost that is holding this bone
    public GhostBehavior mGhost;
    public bool connecting = false;

    [SerializeField]
    private string axisKey; 

    //properties
    Transform IGrabbable.transform { get { return transform; } }
    public Rigidbody Rb { get { return rb; } }
    public BoneGroup Group { get { return group; } }
    public int ID { get; private set; }
    public string AxisKey { get { return axisKey; } private set { axisKey = value; } }

    protected virtual void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    protected virtual void Start()
    {
        //register w/ boneManager
        BoneManager.Instance.Register(this);
        if(collisionHandler == null)
            collisionHandler = BoneManager.Collision;
        group = gameObject.GetComponent<BoneGroup>();
        //before collisions groups will always have a unique ID which the bone can use as it's own
        ID = group.GroupID;
    }

    void IGrabbable.PickedUp()
    {
        if (mGhost)
        {
            mGhost.LostBone();

            // Will - Plays a sound when bones are picked up (for testing purposes only)
            // AudioManager.Instance.PlayTestSound();
        }
        mGhost = null;
        group.ApplyToAll((Bone b, FunctionArgs a)=> { b.rb.freezeRotation = true; b.rb.useGravity = false;});
        BoneManager.Collision.SetPhysicsLayer(this, physicsLayer);
    }

    void IGrabbable.Dropped()
    {
        const float maxReleaseYVelocity = 1.0f;
        //rb.useGravity = true; gravity was initally in picked up

        group.ApplyToAll((Bone b, FunctionArgs a) => { 
            b.GetComponent<CustomGravity>().enabled = true; 
            b.rb.freezeRotation = false;

            Vector3 velocity = Rb.velocity;
            if (Mathf.Abs(velocity.y) > maxReleaseYVelocity)
            {
                Rb.velocity = Vector3.ProjectOnPlane(velocity, Vector3.up) + (Vector3.up * maxReleaseYVelocity);
            }
        });
    }

        
    private void OnCollisionEnter(Collision collision)
    {
        collisionHandler.AddBoneCollision(this, collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bone"))
        {
            Bone colliding = collision.gameObject.GetComponent<Bone>();
            //have the bone with the lower group id send the message to the 
            //handler to avoid redundant messages
            //bones within the same group will not send collision messages
            if (colliding && group.GroupID < colliding.group.GroupID)
                collisionHandler.RemoveBoneCollision(this, colliding);
        }
    }

}
