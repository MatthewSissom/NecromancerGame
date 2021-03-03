using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This touch has an active bone which floats near the 
//players finger
public class BoneMovingTouch : TouchProxy
{
    public IGrabbable activeObj { private set; get; }
    //public BoneGroup.applyToAllType applyToActiveGroup;

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

    //rotation
    Coroutine decelerationRoutine;

    public void SetActive(IGrabbable bone)
    {
        activeObj = bone;
    }

    public override void Move(Vector3 pos, float rad)
    {
        speed = (pos - previousLocation).magnitude;
        previousLocation = transform.position;
        base.Move(pos, rad);
        //lightTransform.transform.position = transform.position + offset;
    }

    public void CancleStopRotation()
    {
        if (decelerationRoutine != null)
            StopCoroutine(decelerationRoutine);
    }

    public void StopRotation(float deceleration)
    {
        IEnumerator StopRoutine()
        {
            float time= 0;
            float magnitude = 1;
            while (isActiveAndEnabled && activeObj != null && magnitude > 0)
            {
                time = Time.deltaTime;
                magnitude = activeObj.Rb.angularVelocity.magnitude;
                float scaleFactor = 1 - (deceleration * time / magnitude);
                if (scaleFactor < 0)
                    activeObj.Rb.angularVelocity = new Vector3();
                else
                    activeObj.Rb.angularVelocity = activeObj.Rb.angularVelocity * scaleFactor;
                yield return null;
            }
        }
        decelerationRoutine = StartCoroutine(StopRoutine());
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (activeObj != null)
            return;
        IGrabbable b = other.GetComponentInParent<GrabbableGroup>();
        if (b != null)
        {
            (b as IGrabbable).PickedUp();
            SetActive(b);
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
            Vector3.ClampMagnitude(toProxy, maxVelocity);
            activeObj.Rb.velocity = toProxy;
        }
        //the rad of the touch collider quickly increases to the normal size when first being reenabled
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

        if (activeObj != null)
        {
            activeObj.Dropped();
        }
    }
}
