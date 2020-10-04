using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bone : MonoBehaviour
{
    private boneGroup group;
    private Rigidbody rb;
    public GhostBehavior mGhost;

    public Rigidbody Rb { get { return rb; }}
    public boneGroup Group { get { return group; } }
    //public GameObject particleEffect;

    // Start is called before the first frame update
    void Awake()
    {
        //get values
        rb = gameObject.GetComponent<Rigidbody>();
        group = gameObject.GetComponent<boneGroup>();
        if(!group) group = (boneGroup)gameObject.AddComponent(typeof(boneGroup));
    }

    private void Start()
    {
        //particleEffect = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/ParticleFX/Combine/CombineFX.prefab", typeof(GameObject));
    }

    public void PickedUp()
    {
        if (mGhost)
        {
            mGhost.LostBone();
        }
        mGhost = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!mGhost)
        {
            bone colliding = collision.gameObject.GetComponent<bone>();
            //if colliding with a bone and not in the same group already
            //then connect the two bones together
            if (colliding && group.GroupID < colliding.group.GroupID)
            {
                //---Bone Connection---//

                //update group trees
                boneGroup.combineGroups(group, colliding.group);
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
                Instantiate(particleEffect, jointWorldPoint, Quaternion.identity);
            }
        }
    }
}
