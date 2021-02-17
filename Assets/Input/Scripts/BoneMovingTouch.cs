using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This touch has an active bone which floats near the 
//players finger
public class BoneMovingTouch : TouchProxy
{
    public IGrabbable activeObj { private set; get; }
    public BoneGroup.applyToAllType applyToActiveGroup;

    //the offset of the bone from the touch on the screen,
    //makes the bone more visable
    public Vector3 offset;

    //when the bone is farther away than this threshold it will only move straight up
    //afterwards it will move directly to the proxy
    public float heightThreshold;

    //particle fx
    private GameObject lightTransform;
    private ParticleSystem touchLights;

    //the collider of this touch, used to find bones underneath it
    private BoxCollider myVolume;
    //rad starts smaller and grows larger over a fraction of a second to avoid picking up bones on
    //the outside of the rad instead of bones closer to the center
    private float radMult;

    //speed
    public float speed;
    public Vector3 previousLocation;

    //angular velocity
    public float angularVelocity;
    float angularDrag = 1;
    float angularDragMult = 50;
    bool dragMultApplied = false;
    Vector3 axisOfRotation;

    public void SetBone(Bone bone)
    {
        if (activeObj != null)
        {
            activeObj = bone;
            applyToActiveGroup = bone.Group.ApplyToAll;
        }
    }

    public override void Move(Vector3 pos, float rad)
    {
        speed = (pos - previousLocation).magnitude;
        previousLocation = transform.position;
        base.Move(pos, rad);
        //lightTransform.transform.position = transform.position + offset;
    }

    public void SetAxisOfRotation(Vector3 newAxis)
    {
        axisOfRotation = newAxis;
        angularVelocity = 0;
    }

    public void ApplyAngularDragMult()
    {
        if(!dragMultApplied)
            angularDrag *= angularDragMult;
        dragMultApplied = true;
    }
    public void RemoveAngularDragMult()
    {
        if(dragMultApplied)
            angularDrag /= angularDragMult;
        dragMultApplied = false;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Bone b = other.GetComponentInParent<Bone>();
        if (b && !b.connecting)
        {
            (b as IGrabbable).PickedUp();
            SetBone(b);
            //touchLights.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (activeObj != null)
        {
            //-move the active object to the proxy-//

            const float maxVelocity = 7.0f;
            const float baseMult = 20;
            //find movement vector
            Vector3 toProxy = (transform.position + offset - activeObj.transform.root.position) * baseMult;
            //move straight up if far away from the proxy
            //if (toProxy.y > heightThreshold)
            //{
            //    toProxy = new Vector3(0, toProxy.y, 0);
            //}
            Vector3.ClampMagnitude(toProxy, maxVelocity);

            void SetVelocity(Bone toApply, FunctionArgs e)
            {
                toApply.Rb.velocity = toProxy;
            }
            applyToActiveGroup(SetVelocity);

            //-rotate the bone group-//

            applyToActiveGroup((Bone toApply, FunctionArgs e) =>
            {
                toApply.transform.RotateAround(activeObj.transform.root.position, axisOfRotation, angularVelocity*Time.deltaTime);
            });
            angularVelocity -= Mathf.Sign(angularVelocity) * Mathf.Min(angularDrag * Time.deltaTime, Mathf.Abs(angularVelocity));
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
        activeObj = null;
        myVolume = gameObject.GetComponent<BoxCollider>();
        lightTransform = ParticleManager.CreateEffect("TouchLight", transform.position);
        touchLights = lightTransform.GetComponent<ParticleSystem>();
        radMult = .1f;
    }

    public void OnEnable()
    {
        activeObj = null;
        radMult = .1f;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        //touchLights.Stop();
        angularVelocity = 0;

        if (activeObj != null)
        {
            activeObj.Dropped();
        }
    }
}
