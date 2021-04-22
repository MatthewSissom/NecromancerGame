using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GhostPhysics : MonoBehaviour
{
    //direction values
    [SerializeField]
    Vector3 target = new Vector3(0,0,1);
    float targetRad;
    public delegate void ArrivalCallbackType();
    public  ArrivalCallbackType ArrivalCallback = null; 

    //speed values
    [Header("Speed")]
    [SerializeField]
    private float maxAcceleration = 0;
    public  float Acceleration
    {
        get { return acceleration; }
        set { acceleration = Mathf.Max(value, maxAcceleration); }
    }
    private float acceleration;
    [SerializeField]
    private float maxSpeed = 0;
    private float targetSpeed = 0;
    private bool useArrivalForces = false;

    //angular velocity values
    [Header("Angular Velocity")]
    [SerializeField]
    private float maxAngularVelocity = default;
    [SerializeField]
    private float torque = 0;
    private Vector3 targetForward;

    //Stabilization values
    [Header("Stabilization")]
    private float targetHeight = 0;
    [SerializeField]
    private float maxYSpeed = default;
    [SerializeField]
    private float wobbleTollerance = 0;
    [SerializeField]
    private float ballanceRotationThreshold = 0;
    [SerializeField]
    private float bobTollerance = 0;
    [SerializeField]
    private float ballanceHeightThreshold = 0;
    private Vector3 up = new Vector3(0, 1, 0);
    [SerializeField]
    List<PID> movementPIDs;

    private bool rotating = false;
    private bool moving = false;
    private bool bobbing = true;
    private bool targetOriented = false;

    //component refrences
    public Rigidbody rb { get; private set; }

    #region Interface

    public void MoveToPosition(Vector3 destination, float targetRad, bool arivalForces = false)
    {
        moving = true;
        targetOriented = true;
        target = destination;
        this.targetRad = targetRad;
        useArrivalForces = arivalForces;
        if(useArrivalForces)
        {
            foreach (PID pid in movementPIDs)
                pid.Reset();
        }
    }

    public void LookAt(Vector3 location)
    {
        location = location - transform.position;
        targetForward = new Vector3(location.x, 0, location.z).normalized;
    }

    public void RotateTo(Vector3 forward)
    {
        targetForward = forward;
    }

    //moves the ghost in it's up direction
    public void Jump(float mult, Rigidbody rb = null)
    {
        if (!rb)
            rb = this.rb;
        rb.velocity -= Vector3.Project(rb.velocity, transform.up);
        rb.AddForce(transform.up * maxAcceleration * mult,ForceMode.VelocityChange);
    }

    #endregion

    #region InternalUpdate

    // Update is called once per frame
    void Update()
    {
        if (Balance() && bobbing)
        {
            //rb.AddForce((Mathf.Sin(Time.time*2)) *new Vector3(0,0.005f,0));
        }
        Balance();
        if(useArrivalForces)
            UpdateVelocityPID(target - transform.position, Time.deltaTime);
        else
            UpdateVelocity(target - transform.position);
    }

    //adjusts ghosts up and forward vectors
    //returns if the ghost is close to the proper orientation
    private bool Balance()
    {
        bool balanced = true;
        float angleFromTargetUp = Mathf.Abs(Mathf.Acos(Vector3.Dot(gameObject.transform.up, up))) * 180 / Mathf.PI;
        float angleFromTargetForward = Mathf.Abs(Mathf.Acos(Vector3.Dot(gameObject.transform.forward, targetForward))) * 180 / Mathf.PI;
        if (!rotating && (angleFromTargetUp > wobbleTollerance || angleFromTargetForward > wobbleTollerance))
        {
            StartCoroutine(Reorient());
            if (angleFromTargetUp > ballanceRotationThreshold)
            {
                if (balanced)
                {
                    balanced = false;
                }
            }
        }
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
            MoveToPosition(target,targetRad);
            if (Mathf.Abs(transform.position.y - target.y) > ballanceHeightThreshold)
            {
                if (balanced)
                {
                    balanced = false;
                }
            }
        }

        return balanced;
    }

    void UpdateVelocityPID(Vector3 toTarget, float dt)
    {
        Vector3 correction = new Vector3(
            movementPIDs[0].Update(toTarget[0], dt),
            movementPIDs[1].Update(toTarget[1], dt),
            movementPIDs[2].Update(toTarget[2], dt)
            );
        if (correction.magnitude < 0.0001f && moving)
        {
            moving = false;
            targetOriented = false;
            rb.velocity = new Vector3();
            transform.position += toTarget;
            ArrivalCallback();
            return;
        }
        
        rb.velocity += Vector3.ClampMagnitude(correction, dt * acceleration);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        if (targetOriented)
            targetForward = new Vector3(toTarget.x, 0, toTarget.z);
    }

    void UpdateVelocity(Vector3 toTarget)
    {
        float length = toTarget.magnitude;
        if (length > targetRad)
        {
            Vector3 toTargetNormal = toTarget / length;
            Vector3 correction = targetSpeed * toTargetNormal - rb.velocity;
            Vector3 force = correction.normalized * acceleration * Time.deltaTime;

            rb.velocity += force;
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
            if(targetOriented)
                targetForward = new Vector3(toTarget.x, 0, toTarget.z);
        }
        else if (moving)
        {
            targetRad = bobTollerance;
            moving = false;
            targetOriented = false;
            useArrivalForces = true;
            ArrivalCallback();
        }
    }

    #endregion

    #region Coroutines

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

    #endregion

    #region Init

    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        acceleration = maxAcceleration;
        targetSpeed = maxSpeed;
        targetForward = transform.forward;

        if (movementPIDs.Count != 3)
        {
            movementPIDs = new List<PID>();
            for (int i = 0; i < 3; i++)
                movementPIDs.Add(new PID(1, .2f, 2));
        }
    }

    #endregion

}
