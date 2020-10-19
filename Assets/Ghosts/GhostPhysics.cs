using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostPhysics : MonoBehaviour
{
    //direction values
    Vector3 target;

    //speed values
    [Header("Speed")]
    [SerializeField]
    private float maxAcceleration;
    private float acceleration;
    public  float Acceleration
    {
        get { return acceleration; }
        set { acceleration = Mathf.Max(value, maxAcceleration); }
    }
    [SerializeField]
    private float maxSpeed;

    //angular velocity values
    [Header("Angular Velocity")]
    [SerializeField]
    private float maxAngularVelocity;
    [SerializeField]
    private float torque;

    //Stabilization values
    [Header("Stabilization")]
    private float targetHeight;
    [SerializeField]
    private float maxYSpeed;
    [SerializeField]
    private float wobbleTollerance;
    [SerializeField]
    private float ballanceRotationThreshold;
    [SerializeField]
    private float bobTollerance;
    [SerializeField]
    private float ballanceHeightThreshold;
    private Vector3 up = new Vector3(0, 1, 0);

    bool rotating = false;

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
        if(!Balance())
        {

        }
    }

    //adjusts ghosts up and forward vectors and y valu
    //returns if this ghost is 'balanced'
    private bool Balance()
    {
        bool balanced = true;
        float angleFromTargetUp = Mathf.Abs(Mathf.Acos(Vector3.Dot(gameObject.transform.up, up))) * 180 / Mathf.PI;
        if (angleFromTargetUp > wobbleTollerance)
        {
            float angvel = rb.angularVelocity.magnitude;

            if(angvel > 1)
            {
                rb.angularVelocity *= angvel - torque * Time.deltaTime;
            }
            else if(!rotating)
            {
                RotateToAngle(new Vector3(0,90,0));
            }

            if(angleFromTargetUp > ballanceRotationThreshold)
            {
                balanced = false;
            }
        }
        float angleFromTargetForward = Mathf.Abs(Mathf.Acos(Vector3.Dot(gameObject.transform.up, up))) * 180 / Mathf.PI;
        if(angleFromTargetForward > wobbleTollerance)
        {
            float angvel = rb.angularVelocity.magnitude;

            if (angvel > 1)
            {
                rb.angularVelocity *= angvel - torque * Time.deltaTime;
            }
            else if (!rotating)
            {
                RotateToAngle(new Vector3(0, 90, 0));
            }

            if (angleFromTargetForward > ballanceRotationThreshold)
            {
                balanced = false;
            }
        }
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
            float totalTime = Quaternion.Angle(final, transform.rotation) / torque;
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



    IEnumerator AngularAcceleration(float seconds, Vector3 unitTorque)
    {
        float timer = 0;
        float speed;
        while (timer <= seconds)
        {
            timer += Time.deltaTime;
            speed = rb.angularVelocity.magnitude;
            rb.velocity = (speed + Time.deltaTime * acceleration) / speed * rb.velocity;
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
