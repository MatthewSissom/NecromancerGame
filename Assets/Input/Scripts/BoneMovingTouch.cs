using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This touch has an active bone which floats near the 
//players finger
public class BoneMovingTouch : TouchProxy
{
    public IGrabbable activeWatch { private set; get; }
    public GrabbableGroup activeBone { private set; get; }
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
    public Vector3 primaryMidpoint { private set; get; }
    public Vector3 auxileryAxis { private set; get; }
    

    public void SetActive(IGrabbable bone)
    {
        activeWatch = bone;
    }

    public void SetActive(GrabbableGroup bone, Vector3 primaryMidpoint, Vector3 auxileryAxis)
    {
        activeBone = bone;
        this.primaryMidpoint = primaryMidpoint;
        this.auxileryAxis = auxileryAxis; 
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
            while (isActiveAndEnabled && activeWatch != null && magnitude > 0)
            {
                time = Time.deltaTime;
                magnitude = activeWatch.Rb.angularVelocity.magnitude;
                float scaleFactor = 1 - (deceleration * time / magnitude);
                if (scaleFactor < 0)
                    activeWatch.Rb.angularVelocity = new Vector3();
                else
                    activeWatch.Rb.angularVelocity = activeWatch.Rb.angularVelocity * scaleFactor;
                yield return null;
            }
        }
        decelerationRoutine = StartCoroutine(StopRoutine());
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (activeWatch != null)
            return;
        IGrabbable b = null; //other.GetComponentInParent<GrabbableGroup>(); This will be to grab the stopwatch.
        if(b != null)
        {
            SetActive(b);
            return;
        }
        GrabbableGroup c = other.GetComponentInParent<GrabbableGroup>();
        if (c != null && !c.isRoot && (!c.isAttached || c.isLeaf))
        {
            c.PickedUp();
            SetActive(c, c.PrimaryMidpoint, c.AuxilieryAxis);
            StopRotation(1000 * Mathf.Deg2Rad);
            //touchLights.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (activeBone != null)
        {
            //-move the active object to the proxy-//

            const float maxVelocity = 7.0f;
            const float baseMult = 20;
            //find movement vector
            Vector3 toProxy = (transform.position + offset - activeBone.transform.root.position) * baseMult;
            Vector3.ClampMagnitude(toProxy, maxVelocity);
            activeBone.Rb.velocity = toProxy;
        }else if(activeWatch != null)
        {
            //TODO change active watch movement as needed


            //-move the active object to the proxy-//

            const float maxVelocity = 7.0f;
            const float baseMult = 20;
            //find movement vector
            Vector3 toProxy = (transform.position + offset - activeBone.transform.root.position) * baseMult;
            Vector3.ClampMagnitude(toProxy, maxVelocity);
            activeBone.Rb.velocity = toProxy;
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
        activeWatch = null;
        myVolume = gameObject.GetComponent<BoxCollider>();
        //lightTransform = ParticleManager.CreateEffect("TouchLight", transform.position);
        //touchLights = lightTransform.GetComponent<ParticleSystem>();
        radMult = .1f;
    }

    public void OnEnable()
    {
        activeWatch = null;
        radMult = .1f;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (activeBone != null)
        {
            activeBone.Dropped();
        }
        activeBone = null;
    }
}
