using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour
{
    const int physicsLayer = 10;

    private BoneGroup group;
    private Rigidbody rb;
    public GhostBehavior mGhost;

    // Will's code
    //private ThemeManager themeManager;
    private string theme;
    // end

    public Rigidbody Rb { get { return rb; } }
    public BoneGroup Group { get { return group; } }
    public string Theme 
    { 
        get { return theme; } 
        set { theme = value; } 
    }

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
        group.applyToAll((Bone b, FunctionArgs a)=> { b.rb.freezeRotation = true; b.rb.useGravity = false;});
        BoneManager.Instance.SetBoneLayer(this, physicsLayer,0.25f);
    }

    public void Dropped()
    {
        const float maxReleaseYVelocity = 1.0f;
        //rb.useGravity = true; gravity was initally in picked up

        group.applyToAll((Bone b, FunctionArgs a) => { 
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
        if (collision.gameObject.CompareTag("Bone"))
        {
            Bone colliding = collision.gameObject.GetComponent<Bone>();
            //if colliding with a bone and not in the same group already
            //then connect the two bones together
            if (colliding && group.GroupID < colliding.group.GroupID)
            {
                //---SFX---//
                //FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/BoneConnections");
                //FMODUnity.RuntimeManager.PlayOneShot(themeManager.GetThemeSoundPath(theme));

                //---Bone Connection---//

                //update group trees
                BoneGroup.combineGroups(group, colliding.group);

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
        else if (collision.gameObject.CompareTag("Horizontal"))
        {
            rb.velocity = new Vector3(0,rb.velocity.y,0);
            rb.gameObject.GetComponent<CustomGravity>().enabled = false;
            rb.useGravity = true;
        }
    }
}
