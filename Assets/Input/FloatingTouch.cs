using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This touch has an active bone which floats near the 
//players finger
public class FloatingTouch : TouchProxy
{
    bone activeBone;
    public boneGroup.applyToAllType applyToAll;

    public Vector3 offset;
    public float heightThreshold;

    private GameObject lightParticle;

    //speed
    public float speed;
    public Vector3 previousLocation;

    // Start is called before the first frame update
    void Awake()
    {
        activeBone = null;
        lightParticle = ParticleManager.CreateEffect("TouchLight",transform.position);
    }

    public override void Move(Vector3 pos, float rad)
    {
        speed = (pos - previousLocation).magnitude;
        previousLocation = transform.position;
        base.Move(pos, rad);
        lightParticle.transform.position = transform.position + offset;
    }

    public void SetBone(bone bone)
    {
        if(!activeBone)
        {
            activeBone = bone;
            applyToAll = bone.Group.applyToAll;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //move the active object to the proxy
        const float maxVelocity = 3.0f;
        const float baseMult = 10;

        Vector3 toProxy = (transform.position + offset - activeBone.transform.position) * baseMult;
        if(toProxy.y > heightThreshold)
        {
            toProxy = new Vector3(0, toProxy.y, 0);
        }
        Vector3.ClampMagnitude(toProxy, maxVelocity);

        void SetVelocity(bone toApply, FunctionArgs e)
        {
            toApply.Rb.velocity = toProxy;
            toApply.Rb.angularVelocity = new Vector3();
        }
        applyToAll(SetVelocity);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Destroy(lightParticle);
        if (activeBone)
        {
            //limit the upwards velocity of bones
            void clampYVel(bone toApply, FunctionArgs e)
            {
                const float maxReleaseYVelocity = 1.0f;
                Vector3 velocity = toApply.Rb.velocity;
                if (Mathf.Abs(velocity.y) > maxReleaseYVelocity)
                {
                    toApply.Rb.velocity = Vector3.ProjectOnPlane(velocity, Vector3.up) + (Vector3.up * maxReleaseYVelocity);
                }
            }
            activeBone.Group.applyToAll(clampYVel);
        }
    }
}
