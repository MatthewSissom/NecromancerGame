using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GhostPhysics : MonoBehaviour
{
    //direction values
    Vector3 target = new Vector3(0,0-1);

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

    //angular velocity values
    [Header("Angular Velocity")]
    [SerializeField]
    private float maxAngularVelocity;
    [SerializeField]
    private float torque = 0;

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

    private bool stoppingRotation = false;
    private bool rotating = false;

    //component refrences
    Rigidbody rb;


    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        acceleration = maxAcceleration;
    }

    // Update is called once per frame
    void Update()
    {
            if (!Balance())
            {

            }
    }

    //adjusts ghosts up and forward vectors and y valu
    //returns if this ghost is 'balanced'
    private bool Balance()
    {
        bool balanced = true;
        float angleFromTargetUp = Mathf.Abs(Mathf.Acos(Vector3.Dot(gameObject.transform.up, up))) * 180 / Mathf.PI;
        if (!stoppingRotation && !rotating && angleFromTargetUp > wobbleTollerance)
        {
            IEnumerator Reorient()
            {
                yield return StartCoroutine(StopRotation());
                Vector3 toTarget = target - transform.position;
                toTarget.y = 0;
                toTarget.Normalize();
                RotateToAngle(new Vector3(0, Mathf.Rad2Deg*Mathf.Atan2(toTarget.z, toTarget.x), 0));
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
        if (Mathf.Abs(transform.position.y - targetHeight) > bobTollerance)
        {


            if (Mathf.Abs(transform.position.y - targetHeight) > ballanceHeightThreshold)
            {
                balanced = false;
            }
        }

        return balanced;
    }

    public void MoveToPosition(Vector3 destination)
    {
        IEnumerator MoveTo()
        {
            target = destination;

            Vector3 toTarget;
            float length = 5;
            float speed;

            while (length > .01f)
            {
                toTarget = destination - transform.position;
                length = toTarget.magnitude;
                speed = rb.velocity.magnitude;

                if (speed * speed / (2 * acceleration) > length)
                {
                    toTarget *= -1;
                }

                Vector3 force = toTarget.normalized * maxSpeed - rb.velocity;
                force = Vector3.ClampMagnitude(force, acceleration);
                rb.AddForce(force);
                yield return null;
            }

            rb.velocity = new Vector3();

            yield break;
        }
        StartCoroutine(MoveTo());
    }

    //rotation is equal to the number of degrees needed to rotate on each axis
    public void RotateToAngle(Vector3 rotation)
    {
        Quaternion final = Quaternion.Euler(rotation);
        Quaternion inital = transform.rotation;
        IEnumerator RotateTo()
        {
            rotating = true;
            float totalTime = Quaternion.Angle(final, transform.rotation) / torque /  30;
            float currentTime = 0;

            while (currentTime <= totalTime)
            {
                transform.rotation = Quaternion.Slerp(inital, final, currentTime / totalTime);
                currentTime += Time.deltaTime;
                yield return null;
            }
            rotating = false;
            yield break;
        }
        StartCoroutine(RotateTo());
    }

    IEnumerator StopRotation()
    {
        stoppingRotation = true;
        Vector3 angularVelocity = rb.angularVelocity;
        float angspeed = angularVelocity.magnitude;
        if (angspeed < torque * Time.deltaTime)
        {
            rb.angularVelocity = new Vector3();
            stoppingRotation = false;
            yield break;
        }

        int getSigniture()
        { return Math.Sign(angularVelocity.x) + Math.Sign(angularVelocity.y) * 2 + Math.Sign(angularVelocity.z) * 4; }

        int signiture = getSigniture();

        while (signiture == getSigniture())
        {
            angularVelocity = rb.angularVelocity;
            angspeed = angularVelocity.magnitude;
            rb.angularVelocity *= (angspeed - torque * Time.deltaTime)/angspeed;
            yield return null;
        }

        rb.angularVelocity = new Vector3();
        stoppingRotation = false;
        yield break;
    }

    IEnumerator AngularAcceleration(float seconds, Vector3 unitTorque)
    {
        float timer = 0;
        float speed;
        while (timer <= seconds)
        {
            timer += Time.deltaTime;
            speed = rb.angularVelocity.magnitude;
            rb.angularVelocity = (speed + Time.deltaTime * torque) / speed * rb.angularVelocity;
            yield return null;
        }
        yield break;
    }

    IEnumerator Accelerate(float seconds, Vector3 direction)
    {
        float timer = 0;
        float speed;
        while(timer <= seconds)
        {
            timer += Time.deltaTime;
            speed = rb.velocity.magnitude;
            rb.velocity = (speed + Time.deltaTime * acceleration) / speed * rb.velocity;
            yield return null;
        }
        yield break;
    }
}
