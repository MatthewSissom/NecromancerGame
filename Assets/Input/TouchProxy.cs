using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchProxy : MonoBehaviour
{
    bone activeBone;

    private float height;

    private BoxCollider myVolume;
    //rad starts smaller and grows larger over a fraction of a second to avoid picking up bones on
    //the outside of the rad instead of bones closer to the center
    private float radMult;

    public float radius { get; set; }

    public Vector3 offset;

    public boneGroup.applyToAllType applyToAll;

    public delegate void destroyCallback();
    public event destroyCallback DestroyEvent;

    // Start is called before the first frame update
    void Start()
    {
        myVolume = gameObject.GetComponent<BoxCollider>();
        height = transform.position.y;
        radMult = .1f;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (activeBone)
        {
            //move the active object to the proxy
            const float maxVelocity = 3.0f;
            const float baseMult = 10;

            Vector3 toProxy = (transform.position + offset- activeBone.transform.position) * baseMult;
            Vector3.ClampMagnitude(toProxy, maxVelocity);

            void SetVelocity(bone toApply, FunctionArgs e)
            {
                toApply.Rb.velocity = toProxy;
                toApply.Rb.angularVelocity = new Vector3();
            }
            applyToAll(SetVelocity);
        }
        else
        {
            if (radMult < 1)
                radMult += Time.deltaTime * 5;
            transform.up = Camera.main.transform.position - transform.position;
            myVolume.size = new Vector3(radius * 2 * radMult, myVolume.size.y, radius * 2 * radMult);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        bone b = other.GetComponentInParent<bone>();
        if (b)
        {
            myVolume.enabled = false;
            b.PickedUp();
            activeBone = b;
            applyToAll = b.Group.applyToAll;
        }
    }

    protected virtual void OnDestroy()
    {
        DestroyEvent?.Invoke();
        if (activeBone)
        {
            //limit the upwards velocity of bones
            void clampYVel(bone toApply, FunctionArgs e)
            {
                const float maxReleaseYVelocity = 1.0f;
                Vector3 velocity = toApply.Rb.velocity;
                if (Mathf.Abs(velocity.y) > maxReleaseYVelocity)
                {
                    toApply.Rb.velocity = Vector3.ProjectOnPlane(velocity, Vector3.up) + (Vector3.up * maxReleaseYVelocity);
                }
            }
            activeBone.Group.applyToAll(clampYVel);
        }
    }
}

//drag init code//
//[SerializeField]
//private float dragPower;
//bool dragging = false;
//Vector3 localDragPoint;

//drag pick up code//
//if(Input.GetMouseButton(1) && clickDisableTimer > clickDisableTime)
//{
//    RaycastHit hitInfo;
//    if (Physics.Raycast(transform.position, fromCamera, out hitInfo))
//    {
//        activeBone = hitInfo.collider.gameObject.GetComponentInParent<bone>();
//        if (!activeBone)
//            { return; }

//        //check to see if the bone is on the conveyor
//        if (activeBone.Group.GroupID == 0)
//        {
//            activeBone.Group.removeFromConvayer();
//        }

//        localDragPoint = activeBone.transform.worldToLocalMatrix.MultiplyPoint(hitInfo.point);
//        dragging = true;
//    }
//    clickDisableTimer = 0;
//}

//drag movement code//
//else
//{
//    Vector3 worldCord = activeBone.transform.localToWorldMatrix.MultiplyPoint(localDragPoint);
//    Vector3 force = (- worldCord + Camera.main.transform.position + Vector3.Project(worldCord - Camera.main.transform.position, fromCamera)).normalized * Time.deltaTime * dragPower;

//    activeBone.Rb.AddForceAtPosition(force, worldCord);
//}
