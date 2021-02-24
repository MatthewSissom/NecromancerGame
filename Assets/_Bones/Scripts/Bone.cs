using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour, IGrabbable
{
    //physics
    const int physicsLayer = 10;
    protected Rigidbody rb;

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
        BoneManager.Instance.SetPhysicsLayer(this, physicsLayer,0.25f);
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

    private void OnTriggerEnter(Collider other)
    {
        
    }

        /*
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bone"))
        {
            Bone colliding = collision.gameObject.GetComponent<Bone>();
            //if colliding with a bone and not in the same group already
            //then connect the two bones together
            if (
                (connecting || colliding.connecting) 
                && colliding && group.GroupID < colliding.group.GroupID
            )
            {
                //---SFX---//
                FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/BoneConnections");

                //---Bone Connection---//

                ////update group trees
                //BoneGroup.CombineGroups(group, colliding.group);

                ////get joint info from collision
                //Vector3 jointWorldPoint;
                //Vector3 jointDirection;
                //if (collision.contactCount == 1)
                //{
                //    ContactPoint contact = collision.GetContact(0);
                //    jointWorldPoint = contact.point;
                //    jointDirection = contact.normal;
                //}
                //else
                //{
                //    jointWorldPoint = new Vector3();
                //    jointDirection = new Vector3();
                //    ContactPoint[] points = new ContactPoint[collision.contactCount];
                //    collision.GetContacts(points);
                //    foreach (ContactPoint c in points)
                //    {
                //        jointWorldPoint += c.point;
                //        jointDirection += c.normal;
                //    }
                //    jointWorldPoint /= collision.contactCount;
                //    jointDirection /= collision.contactCount;
                //}

                ////create joint
                //SpringJoint newJoint = gameObject.AddComponent(typeof(SpringJoint)) as SpringJoint;
                //newJoint.anchor = transform.InverseTransformPoint(jointWorldPoint);
                //newJoint.connectedBody = colliding.Rb;
                //newJoint.autoConfigureConnectedAnchor = false;
                //newJoint.connectedAnchor = colliding.transform.InverseTransformPoint(jointWorldPoint);
                //newJoint.spring = 500;
                //newJoint.damper = 10;
                //newJoint.minDistance = 0.0f;
                //newJoint.maxDistance = .025f;
                //newJoint.enableCollision = true;

                ////create particle effect
                //ParticleManager.CreateEffect("CombineFX", jointWorldPoint);
            }
        }
        else if (collision.gameObject.CompareTag("Horizontal"))
        {
            rb.velocity = new Vector3(0,rb.velocity.y,0);
            rb.gameObject.GetComponent<CustomGravity>().enabled = false;
            rb.useGravity = true;
        }
    }
        */
}
