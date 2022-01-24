using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopwatchLid : MonoBehaviour, IGrabbable
{
    Transform IGrabbable.transform { get { return transform; } }
    public Rigidbody Rb { get { return proxyRb; } }

    public float rbReturnSpeed;

    public Transform hinge;
    public GameObject lidProxy;
    private Rigidbody proxyRb;
    public GameObject rbMarker;

    bool checkForEnd = false;
    bool returnRb = false;
    float currentAngle = 0;

    public event System.Action LidClosed;

    public void Dropped()
    {
        checkForEnd = true;
        returnRb = true;
        Rb.useGravity = false;
    }

    public void PickedUp()
    {
        checkForEnd = false;
        returnRb = false;
        Rb.useGravity = false;
    }

    float RotationFromRbPos()
    {
        float tableComponent = Vector3.Dot(Rb.position, hinge.right);
        float upComponent = Vector3.Dot(Rb.position, hinge.up);
        //subtract from height to allow rotation at extreem distances
        upComponent/=3;
        upComponent -= 2 * tableComponent;

        float angle = Mathf.Atan2(upComponent, tableComponent);
        return Mathf.Min(Mathf.PI, Mathf.Max(angle, 0))*Mathf.Rad2Deg;
    }

    // Update is called once per frame
    void Update()
    {
        float newAngle = RotationFromRbPos();
        float deltaAngle = newAngle - currentAngle;
        currentAngle = newAngle;
        transform.RotateAround(hinge.transform.position, hinge.forward, deltaAngle);

        if (checkForEnd && newAngle == 0)
        {
            LidClosed?.Invoke();
            checkForEnd = false;
        }
        if (returnRb)
        {
            float distance = Time.deltaTime * rbReturnSpeed;
            if (Rb.velocity.magnitude > 0)
            {
                Rb.velocity = Mathf.Min(proxyRb.velocity.magnitude - distance, 0) * Rb.velocity.normalized;
            }
            else
            {
                var delta = rbMarker.transform.position - Rb.transform.position;
                var magnitude = delta.magnitude;
                if (magnitude < distance)
                {
                    Rb.transform.position = rbMarker.transform.position;
                    returnRb = false;
                    checkForEnd = false;
                }
                Rb.transform.position += delta * Mathf.Max(distance,magnitude/200) / magnitude;
            }
        }
    }

    void Start()
    {
        proxyRb = lidProxy.GetComponent<Rigidbody>();
    }
}
