using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour
{
    const int physicsLayer = 10;

    private BoneGroup group;
    private Rigidbody rb;
    public GhostBehavior mGhost;

    public Rigidbody Rb { get { return rb; } }
    public BoneGroup Group { get { return group; } }

    // Start is called before the first frame update
    void Awake()
    {
        //get values
        rb = gameObject.GetComponent<Rigidbody>();
        group = gameObject.GetComponent<BoneGroup>();
        if (!group) group = (BoneGroup)gameObject.AddComponent(typeof(BoneGroup));
    }

    public void PickedUp()
    {
        if (mGhost)
        {
            mGhost.LostBone();
        }
        mGhost = null;
        rb.useGravity = true;
        rb.freezeRotation = true;
        BoneManager.Instance.SetBoneLayer(this, physicsLayer,0.25f);
    }

    public void Dropped()
    {
        const float maxReleaseYVelocity = 1.0f;
        Vector3 velocity = Rb.velocity;
        if (Mathf.Abs(velocity.y) > maxReleaseYVelocity)
        {
            Rb.velocity = Vector3.ProjectOnPlane(velocity, Vector3.up) + (Vector3.up * maxReleaseYVelocity);
        }
        rb.freezeRotation = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Bone colliding = collision.gameObject.GetComponent<Bone>();
        //if colliding with a bone and not in the same group already
        //then connect the two bones together
        if (colliding && group.GroupID < colliding.group.GroupID)
        {
            //---Bone Connection---//

            //update group trees
            BoneGroup.combineGroups(group, colliding.group);
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/BoneConnections");

            //get joint info from collision
            Vector3 jointWorldPoint;
            Vector3 jointDirection;
            if (collision.contactCount == 1)
            {
                ContactPoint contact = collision.GetContact(0);
                jointWorldPoint = contact.point;
                jointDirection = contact.normal;
            }
            else
            {
                jointWorldPoint = new Vector3();
                jointDirection = new Vector3();
                ContactPoint[] points = new ContactPoint[collision.contactCount];
                collision.GetContacts(points);
                foreach (ContactPoint c in points)
                {
                    jointWorldPoint += c.point;
                    jointDirection += c.normal;
                }
                jointWorldPoint /= collision.contactCount;
                jointDirection /= collision.contactCount;
            }

            //create joint
            SpringJoint newJoint = gameObject.AddComponent(typeof(SpringJoint)) as SpringJoint;
            newJoint.anchor = transform.InverseTransformPoint(jointWorldPoint);
            newJoint.connectedBody = colliding.Rb;
            newJoint.autoConfigureConnectedAnchor = false;
            newJoint.connectedAnchor = colliding.transform.InverseTransformPoint(jointWorldPoint);
            newJoint.spring = 500;
            newJoint.damper = 10;
            newJoint.minDistance = 0.0f;
            newJoint.maxDistance = .025f;
            newJoint.enableCollision = true;

            //create particle effect
            ParticleManager.CreateEffect("CombineFX", jointWorldPoint);
        }
    }
}
