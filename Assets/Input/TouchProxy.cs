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

    // Start is called before the first frame update
    void Start()
    {
        myVolume = gameObject.GetComponent<BoxCollider>();
        height = transform.position.y;
        radMult = .1f;
    }

    public void RotateGroup(float angle)
    {
        void rotateY(bone toApply, FunctionArgs e)
        {
            toApply.transform.RotateAround((e as VecPair).vec1, Vector3.up, (e as VecPair).f);
        }
        activeBone.Group.applyToAll(rotateY, new VecPair(transform.position, angle));
    }

    public class VecWraper : FunctionArgs 
    { public Vector3 vec; public VecWraper(Vector3 vec) { this.vec = vec; } }
    public class VecPair : FunctionArgs 
    { public Vector3 vec1; public float f; public VecPair(Vector3 vec1, float f) { this.vec1 = vec1; this.f = f; } }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (activeBone)
        {
            //move the active object to the proxy
            const float maxVelocity = 3.0f;
            const float baseMult = 10;

            Vector3 toProxy = (transform.position - activeBone.transform.position) * baseMult;
            Vector3.ClampMagnitude(toProxy, maxVelocity);
            activeBone.Group.applyToAll(
                (bone toApply, FunctionArgs e) =>
                {
                    toApply.Rb.velocity = ((VecWraper)e).vec;
                    toApply.Rb.angularVelocity = new Vector3();
                },
                new VecWraper(toProxy));
        }
        else
        {
            if (radMult < 1)
                radMult += Time.deltaTime * 5;
            transform.up = Camera.main.transform.position - transform.position;
            myVolume.size = new Vector3(radius * 2 * radMult, myVolume.size.y, radius * 2 * radMult);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        bone b = other.GetComponentInParent<bone>();
        if (b)
        {
            myVolume.enabled = false;
            b.PickedUp();
            activeBone = b;
        }
    }

    protected virtual void OnDestroy()
    {
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
            activeBone.Group.applyToAll(clampYVel, new FunctionArgs());
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
