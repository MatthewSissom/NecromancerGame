using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour
{
    //physics
    protected static BoneCollisionHandler collisionHandler;

    //bone group, used to store relationships
    //between bones
    protected BoneGroup group;
    public bool connecting = false;

    public delegate void AttachedMethod(Bone bone);
    public event AttachedMethod AttachedEvent;

    [SerializeField]
    private string axisKey; 

    //properties
    public BoneGroup Group { get { return group; } }
    public int ID { get; private set; }
    public string AxisKey { get { return axisKey; } private set { axisKey = value; } }

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
        
    public void WasAttached()
    {
        AttachedEvent?.Invoke(this);
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
