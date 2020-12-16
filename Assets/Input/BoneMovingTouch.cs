﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This touch has an active bone which floats near the 
//players finger
public class BoneMovingTouch : TouchProxy
{
    public Bone activeBone { private set; get; }
    public BoneGroup.applyToAllType applyToAll;

    public Vector3 offset;
    public float heightThreshold;

    private GameObject lightTransform;
    private ParticleSystem touchLights;

    private float height;

    private BoxCollider myVolume;
    //rad starts smaller and grows larger over a fraction of a second to avoid picking up bones on
    //the outside of the rad instead of bones closer to the center
    private float radMult;


    //speed
    public float speed;
    public Vector3 previousLocation;

    public void OnEnable()
    {
        activeBone = null;
        radMult = .1f;
    }

    public void OnDisable()
    {
        //touchLights.Stop();

        //limit the upwards velocity of bones
        void clampYVel(Bone toApply, FunctionArgs e)
        {
            const float maxReleaseYVelocity = 1.0f;
            Vector3 velocity = toApply.Rb.velocity;
            if (Mathf.Abs(velocity.y) > maxReleaseYVelocity)
            {
                toApply.Rb.velocity = Vector3.ProjectOnPlane(velocity, Vector3.up) + (Vector3.up * maxReleaseYVelocity);
            }
        }

        if(activeBone)
            activeBone.Group.applyToAll(clampYVel);
    }

    public override void Move(Vector3 pos, float rad)
    {
        speed = (pos - previousLocation).magnitude;
        previousLocation = transform.position;
        base.Move(pos, rad);
        //lightTransform.transform.position = transform.position + offset;
    }

    public void SetBone(Bone bone)
    {
        if (!activeBone)
        {
            activeBone = bone;
            applyToAll = bone.Group.applyToAll;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Bone b = other.GetComponentInParent<Bone>();
        if (b)
        {
            b.PickedUp();
            SetBone(b);
            //touchLights.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (activeBone)
        {
            //move the active object to the proxy
            const float maxVelocity = 7.0f;
            const float baseMult = 20;

            Vector3 toProxy = (transform.position + offset - activeBone.transform.position) * baseMult;
            if (toProxy.y > heightThreshold)
            {
                toProxy = new Vector3(0, toProxy.y, 0);
            }
            Vector3.ClampMagnitude(toProxy, maxVelocity);

            void SetVelocity(Bone toApply, FunctionArgs e)
            {
                toApply.Rb.velocity = toProxy;
                toApply.Rb.angularVelocity = new Vector3();
            }
            applyToAll(SetVelocity);
        }
        else if (radMult < 1)
        {
            radMult += Time.deltaTime * 5;
            transform.up = Camera.main.transform.position - transform.position;
            myVolume.size = new Vector3(radius * 2 * radMult, myVolume.size.y, radius * 2 * radMult);
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        activeBone = null;
        myVolume = gameObject.GetComponent<BoxCollider>();
        lightTransform = ParticleManager.CreateEffect("TouchLight", transform.position);
        touchLights = lightTransform.GetComponent<ParticleSystem>();
        height = transform.position.y;
        radMult = .1f;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Destroy(lightTransform);
    }
}
