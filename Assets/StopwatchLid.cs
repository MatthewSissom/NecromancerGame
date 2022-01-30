using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopwatchLid : MonoBehaviour, IGrabbable
{
    Transform IGrabbable.transform { get { return transform; } }
    public Rigidbody Rb { get { return proxyRb; } }

    [SerializeField]
    private float rbReturnSpeed;

    [SerializeField]
    private Transform hinge;
    [SerializeField]
    private GameObject lidProxy;
    private Vector3 initalProxyPos;
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
        ResetProxy();
    }

    public void PickedUp()
    {
        checkForEnd = false;
        returnRb = false;
    }

    float RotationFromRbPos()
    {
        float tangentToAxisComponent = Vector3.Dot(Rb.position, hinge.right);
        //if(tangentToAxisComponent)

        // construct an imaginary cylinder around the stopwatch lid's axis (r = input height)
        // change the y component of the vector until the vector is on the cylinder, use the angle from
        // horizontal as the stopwatch angle

        float angle = 10;// Mathf.Atan2(upComponent, tableComponent);
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

    void ResetProxy()
    {
        lidProxy.transform.position = initalProxyPos;
        Rb.constraints = Rb.constraints | RigidbodyConstraints.FreezePositionY;
        Rb.velocity = new Vector3();
        Rb.useGravity = false;
    }

    void Init()
    {
        enabled = true;
        proxyRb = lidProxy.GetComponent<Rigidbody>();

        // set proxy to only move in the plane the input manager uses
        Vector3 unadjustedPos = lidProxy.transform.position;
        initalProxyPos = new Vector3(unadjustedPos.x, InputManager.Instance.Height, unadjustedPos.y);
        ResetProxy();
    }

    void Start()
    {
        // add enabling + disabling events 
        GameManager.Instance.AddEventMethod(typeof(GameInit), "end", Init);
        GameManager.Instance.AddEventMethod(typeof(BoneAssembler), "begin", () => { enabled = false; });
        enabled = false;
    }
}
