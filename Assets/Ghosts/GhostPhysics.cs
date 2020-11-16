using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GhostPhysics : MonoBehaviour
{
    //direction values
    [SerializeField]
    Vector3 target = new Vector3(0,0,1);

    //speed values
    [Header("Speed")]
    [SerializeField]
    private float maxAcceleration = 0;
    private float acceleration;
    public  float Acceleration
    {
        get { return acceleration; }
        set { acceleration = Mathf.Max(value, maxAcceleration); }
    }
    [SerializeField]
    private float maxSpeed = 0;
    private float targetSpeed = 0;

    //angular velocity values
    [Header("Angular Velocity")]
    [SerializeField]
    private float maxAngularVelocity;
    [SerializeField]
    private float torque = 0;
    private Vector3 targetForward;

    //Stabilization values
    [Header("Stabilization")]
    private float targetHeight = 0;
    [SerializeField]
    private float maxYSpeed;
    [SerializeField]
    private float wobbleTollerance = 0;
    [SerializeField]
    private float ballanceRotationThreshold = 0;
    [SerializeField]
    private float bobTollerance = 0;
    [SerializeField]
    private float ballanceHeightThreshold = 0;
    private Vector3 up = new Vector3(0, 1, 0);

    private bool rotating = false;
    private bool moving = false;

    //component refrences
    Rigidbody rb;


    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        acceleration = maxAcceleration;
        targetSpeed = maxSpeed;
        targetForward = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        if (Balance())
        {
            targetHeight = Mathf.Sin(Time.time) * bobTollerance * .75f;
        }
        else
        {
            targetHeight = 0;
        }
        UpdateVelocity(target + new Vector3(0, targetHeight, 0) - transform.position);
    }

    //adjusts ghosts up and forward vectors
    //returns if the ghost is close to the proper orientation
    private bool Balance()
    {
        bool balanced = true;
        float angleFromTargetUp = Mathf.Abs(Mathf.Acos(Vector3.Dot(gameObject.transform.up, up))) * 180 / Mathf.PI;
        if (!rotating && angleFromTargetUp > wobbleTollerance)
        {
            IEnumerator Reorient()
            {
                rotating = true;

                Vector3 signedAngleToTarget = new Vector3(1, 1, 1);
                Vector3 localVelocity = new Vector3();
                Vector3 torqueToAdd = new Vector3();

                while (signedAngleToTarget.magnitude > 0.01f || localVelocity.magnitude > 0.01f)
                {
                    signedAngleToTarget.y = Mathf.Deg2Rad * Vector3.SignedAngle(transform.forward, targetForward, transform.up);
                    signedAngleToTarget.x = Mathf.Deg2Rad * Vector3.SignedAngle(transform.up, new Vector3(0, 1, 0), transform.right);
                    signedAngleToTarget.z = Mathf.Deg2Rad * Vector3.SignedAngle(transform.up, new Vector3(0, 1, 0), transform.forward);
                    
                    localVelocity = transform.InverseTransformDirection(rb.angularVelocity);

                    float targetXAngVel = Mathf.Sign(signedAngleToTarget.x) * Mathf.Sqrt(2 * Mathf.Abs(signedAngleToTarget.x) * torque);
                    float targetYAngVel = Mathf.Sign(signedAngleToTarget.y) * Mathf.Sqrt(2 * Mathf.Abs(signedAngleToTarget.y) * torque);
                    float targetZAngVel = Mathf.Sign(signedAngleToTarget.z) * Mathf.Sqrt(2 * Mathf.Abs(signedAngleToTarget.z) * torque);


                    torqueToAdd = new Vector3(targetXAngVel - localVelocity.x, targetYAngVel - localVelocity.y, targetZAngVel - localVelocity.z).normalized * Time.deltaTime * torque;

                    rb.AddRelativeTorque(torqueToAdd);

                    yield return null;
                }

                rb.angularVelocity = new Vector3();
                rotating = false;
                yield break;
            }
            StartCoroutine(Reorient());
            if (angleFromTargetUp > ballanceRotationThreshold)
            {
                balanced = false;
            }
        }
        //float angleFromTargetForward = Mathf.Abs(Mathf.Acos(Vector3.Dot(gameObject.transform.up, up))) * 180 / Mathf.PI;
        //if (!stoppingRotation && !rotating && angleFromTargetForward > wobbleTollerance)
        //{
        //    StartCoroutine(StopRotation());
        //    if (angleFromTargetForward > ballanceRotationThreshold)
        //    {
        //        balanced = false;
        //    }
        //}
        if (Mathf.Abs(transform.position.y - target.y) > bobTollerance)
        {
            if (Mathf.Abs(transform.position.y - target.y) > ballanceHeightThreshold)
            {
                balanced = false;
            }
        }

        return balanced;
    }

    public void UpdateVelocity(Vector3 toTarget)
    {
        float length = toTarget.magnitude;
        float speed = rb.velocity.magnitude;

        if (length > .01f || speed > .5f)
        {
            Vector3 toTargetNormal = toTarget / length;
            //transform.up = new Vector3(0, 1, 0);
            //transform.forward = toTargetNormal;

            ////make sure the ghost is moving in the twards the target
            //if (Vector3.Angle(rb.velocity,toTarget) > 10)
            //{
            //    //TEMP target speed should not be max speed in direction of toTarget
            //    float speedTwards = Vector3.Dot(toTargetNormal, rb.velocity);
            //    if (speedTwards > 0 && speedTwards * speedTwards / (2 * acceleration) >= length)
            //    {
            //        toTargetNormal *= -1;
            //    }
            //    correction = (toTargetNormal * targetSpeed) - rb.velocity;
            //    rb.velocity += (correction.normalized * acceleration * dt);
            //    yield return null;
            //    continue;
            //}

            Vector3 targetVel = Mathf.Min(Mathf.Sqrt(2 * length * acceleration), targetSpeed) * toTarget;
            Vector3 correction = targetVel - rb.velocity;
            Vector3 force = correction.normalized * acceleration * Time.deltaTime;
            rb.velocity += (force);
        }
        else
        {
            moving = false;
        }
    }

    public void MoveToPosition(Vector3 destination)
    {
        moving = true;
        target = destination;
        targetForward = new Vector3(target.x - transform.position.x, 0, target.z - transform.position.z).normalized;

        transform.up = new Vector3(0, 1, 0);
        transform.forward = new Vector3(target.x - transform.position.x, 0, target.z - transform.position.z).normalized;

        IEnumerator MoveTo()
        {
            Vector3 toTarget;
            //length and speed set to any number > .01 for loop
            float length = 1;
            float speed =1;

            //while (length > .01f || speed > .5f)
            //{
            //    toTarget = target + new Vector3(0, targetHeight,0) - transform.position;
            //    length = toTarget.magnitude;
            //    Vector3 toTargetNormal = toTarget / length;
            //    speed = rb.velocity.magnitude;

            //    ////make sure the ghost is moving in the twards the target
            //    //if (Vector3.Angle(rb.velocity,toTarget) > 10)
            //    //{
            //    //    //TEMP target speed should not be max speed in direction of toTarget
            //    //    float speedTwards = Vector3.Dot(toTargetNormal, rb.velocity);
            //    //    if (speedTwards > 0 && speedTwards * speedTwards / (2 * acceleration) >= length)
            //    //    {
            //    //        toTargetNormal *= -1;
            //    //    }
            //    //    correction = (toTargetNormal * targetSpeed) - rb.velocity;
            //    //    rb.velocity += (correction.normalized * acceleration * dt);
            //    //    yield return null;
            //    //    continue;
            //    //}

            //    Vector3 targetVel = Mathf.Min(Mathf.Sqrt(2 * length * acceleration), targetSpeed) * toTarget;
            //    Vector3 correction = targetVel - rb.velocity;
            //    Vector3 force = correction.normalized * acceleration * Time.deltaTime;
            //    rb.velocity +=(force);
            //    yield return null;
            //}
            yield break;
        }
        //StartCoroutine(MoveTo());
    }
}
