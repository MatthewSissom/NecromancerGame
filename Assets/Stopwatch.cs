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


    private bool on = false;
    public bool On { get { return on; } set { on = value; } }
    private Rigidbody rB;
    private CustomGravity mCustomGravity;

    private bool resisting = false;
    

    [SerializeField]
    private GameObject breakEffectObject;
    [SerializeField]
    private GameObject touchEffectObject;


    ParticleSystem breakEffect;
    ParticleSystem touchEffect;


    Transform IGrabbable.transform { get { return transform; } }
    public Rigidbody Rb { get { return rB; } }

    private void Awake()
    {
        angle = 0;
    }
    private void Start()
    {
        rB = GetComponent<Rigidbody>();
        //ResetMass();
        mCustomGravity = GetComponent<CustomGravity>();

        breakEffect = breakEffectObject.GetComponent<ParticleSystem>();
        touchEffect = touchEffectObject.GetComponent<ParticleSystem>();
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
        if (mCustomGravity)
            mCustomGravity.Disable();
        rB.useGravity = false;

        //rB.constraints = (RigidbodyConstraints)112;

        touchEffect.Play();

    }
    public void Dropped()
    {
        touchEffect.Stop();

        const float maxReleaseYVelocity = 1.0f;
        if (mCustomGravity)
            mCustomGravity.Enable();

        //rB.freezeRotation = false;
        gameObject.layer = 12;
        Vector3 velocity = rB.velocity;
        if (Mathf.Abs(velocity.y) > maxReleaseYVelocity)
        {
            rB.velocity = Vector3.ProjectOnPlane(velocity, Vector3.up) + (Vector3.up * maxReleaseYVelocity);
        }
        
    }
    public void ChangeAngle(float change)
    {
        if (!on)
            return;

        if (change < 0 && !resisting) {
            ResistDialBack();
        }
        else
        {
            angleChange += change;
        }
    }

}
