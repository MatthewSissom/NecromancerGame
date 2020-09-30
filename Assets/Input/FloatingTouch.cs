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

    // Start is called before the first frame update
    void Awake()
    {
        activeBone = null;
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
