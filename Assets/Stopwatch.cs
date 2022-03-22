using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stopwatch : MonoBehaviour, IGrabbable
{
    public GameObject hand;
    private float angle = 0;
    public float Angle { get { return angle; } set { angle = value; } }
    [SerializeField]
    private float roundTime = 6;
    [SerializeField]
    private float warningAngle = 240;
    [SerializeField]
    private float resistVelocity = 5.0f;

    private float angleChange = 0;

    private float resistTime;
    private float resistTimer;

    [SerializeField]
    private GameObject homeObject;
    
    [SerializeField]
    private GameObject awayObject;
    

    private Vector3 homePoint;
    public Vector3 Home { get { return homePoint; } }
    private Vector3 awayPoint;
    public Vector3 Away { get { return awayPoint; } }
    private bool returning = false;
    private bool on = false;
    public bool On { get { return on; } set { on = value; } }

    private Rigidbody rB;

    private bool resisting = false;
    [SerializeField]
    float resistClamp = 0.5f;


    [SerializeField]
    private GameObject breakEffectObject;
    [SerializeField]
    private GameObject touchEffectObject;


    private ParticleSystem breakEffect;
    private ParticleSystem touchEffect;

    private const float lerpEndFrame = 30;
    private float lerpCount = 0;

    Transform IGrabbable.transform { get { return transform; } }
    public Rigidbody Rb { get { return rB; } }

    private bool grabbable = true;
    public bool Grabbable { get { return grabbable; } set { grabbable = value; } }
    private bool leaving = false;
    public bool Leaving { get { return leaving; } set { leaving = value; } }
    private bool left = false;
    public bool Left { get { return left; } set { left = value; } }

    private void Awake()
    {
        angle = 0;
    }
    private void Start()
    {
        rB = GetComponent<Rigidbody>();
        //ResetMass();

        breakEffect = breakEffectObject.GetComponent<ParticleSystem>();
        touchEffect = touchEffectObject.GetComponent<ParticleSystem>();
        homePoint = homeObject.transform.position;
        awayPoint = awayObject.transform.position;
    }

    protected void Update()
    {
        if (!on)
            return;
        angleChange += Time.deltaTime * (360 / roundTime);
        
        if (angleChange > 15)
        {
            int tiks = (int)Math.Round(angleChange / 15);
            if (tiks * 15 + angle > 360)
                tiks -= (int)((tiks * 15 + angle - 360) / 15);
            SetHandPercentage(tiks * 15+angle);
            angleChange = 0;
            if (angle > warningAngle)
                AudioManager.Instance.PlayTickTock(true);
            else
                AudioManager.Instance.PlayTickTock(false);
        }
        if(resisting)
        {
            resistTimer += Time.deltaTime;
            if (resistTimer > resistTime + 0.5f)
                resisting = false;
        }
        if (leaving)
        {
            lerpCount++;

            Vector3 interpolation = new Vector3(Mathf.SmoothStep(homePoint.x, awayPoint.x, lerpCount / lerpEndFrame), Mathf.SmoothStep(homePoint.y, awayPoint.y, lerpCount / lerpEndFrame), Mathf.SmoothStep(homePoint.z, awayPoint.z, lerpCount / lerpEndFrame));

            gameObject.transform.position = interpolation;

            if (lerpCount == lerpEndFrame)
            {
                leaving = false;
                left = true;
            }
        }

    }


    public void SetHandPercentage(float newAngle)
    {
        hand.transform.RotateAround(hand.transform.position, Vector3.up, newAngle - angle);
        angle = newAngle;
    }
    /// <summary>
    /// Stopwatch will do some kind of effect to let the player know they can't dial back.
    /// </summary>
    public void ResistDialBack()
    {
        resisting = true;
        resistTime = Time.time;
        resistTimer = resistTime;
        breakEffect.Play();
    }
    public void PickedUp()
    {
        
        rB.useGravity = false;

        //rB.constraints = (RigidbodyConstraints)112;

        touchEffect.Play();

    }
    public void Dropped()
    {
        grabbable = false;
        lerpCount = 0;
        Vector3 startPosition = gameObject.transform.position;
        IEnumerator Routine()
        {
            while (lerpCount != lerpEndFrame)
            {

                lerpCount++;

                Vector3 interpolation = new Vector3(Mathf.SmoothStep(startPosition.x, homePoint.x, lerpCount / lerpEndFrame), Mathf.SmoothStep(startPosition.y, homePoint.y, lerpCount / lerpEndFrame), 
                    Mathf.SmoothStep(startPosition.z, homePoint.z, lerpCount / lerpEndFrame));

                gameObject.transform.position = interpolation;

                yield return null;

            }
            lerpCount = 0;
            left = false;
            leaving = false;
            touchEffect.Stop();
            yield break;
        }
        if (left || leaving) 
            StartCoroutine(Routine());
    }
    public void ChangeAngle(float change)
    {
        if (!on)
            return;

        if (change < -resistClamp && !resisting) {
            ResistDialBack();
        }
        else
        {
            angleChange += change;
        }
    }
    
}
