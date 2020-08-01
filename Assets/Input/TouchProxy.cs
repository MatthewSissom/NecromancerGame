using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchProxy : MonoBehaviour
{
    bone activeBone;

    private float height;

    public virtual bool active { get { return activeBone; }}

    // Start is called before the first frame update
    void Start()
    {
        height = transform.position.y;
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
        if (active)
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
            RaycastHit hitInfo;
            Ray ray = new Ray(transform.position,transform.position - Camera.main.transform.position);
            if (Physics.Raycast(ray, out hitInfo, 10, 1, QueryTriggerInteraction.Collide))
            {
                activeBone = hitInfo.collider.gameObject.GetComponentInParent<bone>();
                if (!activeBone)
                { return; }
                //check to see if the bone is on the conveyor
                if (activeBone.Group.GroupID == 0)
                {
                    activeBone.Group.removeFromConvayer();
                }
            }
        }
    }

    protected virtual void OnDestroy()
    {
        if (active)
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
