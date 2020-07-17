using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseProxy : MonoBehaviour
{
    bone activeBone;

    [SerializeField]
    private float height;
    [SerializeField]
    private GameObject bonePref;

    [SerializeField]
    private float dragPower;

    bool dragging = false;
    Vector3 localDragPoint;

    private float clickDisableTimer = 0;
    private const float clickDisableTime = .3f;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, height, 0);
    }

    public class VecWraper : FunctionArgs 
    { public Vector3 vec; public VecWraper(Vector3 vec) { this.vec = vec; } }
    public class VecPair : FunctionArgs 
    { public Vector3 vec1; public float f; public VecPair(Vector3 vec1, float f) { this.vec1 = vec1; this.f = f; } }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.transform.position.y-height;
        transform.position = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 fromCamera = transform.position - Camera.main.transform.position;

        //check for bone to pick up if none is active
        if(!activeBone)
        {
            if(Input.GetMouseButton(0) && clickDisableTimer > clickDisableTime)
            {
                RaycastHit hitInfo;
                Ray ray = new Ray(transform.position, fromCamera);
                if(Physics.Raycast(ray, out hitInfo, 10, 1,  QueryTriggerInteraction.Collide))
                {
                    activeBone = hitInfo.collider.gameObject.GetComponentInParent<bone>();
                    if (!activeBone) 
                        { return; }
                    //check to see if the bone is on the conveyor
                    if(activeBone.Group.GroupID == 0)
                    {
                        activeBone.Group.removeFromConvayer();
                    }
                    dragging = false;
                }
                clickDisableTimer = 0;
            }
            if(Input.GetMouseButton(1) && clickDisableTimer > clickDisableTime)
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(transform.position, fromCamera, out hitInfo))
                {
                    activeBone = hitInfo.collider.gameObject.GetComponentInParent<bone>();
                    if (!activeBone)
                        { return; }

                    //check to see if the bone is on the conveyor
                    if (activeBone.Group.GroupID == 0)
                    {
                        activeBone.Group.removeFromConvayer();
                    }

                    localDragPoint = activeBone.transform.worldToLocalMatrix.MultiplyPoint(hitInfo.point);
                    dragging = true;
                }
                clickDisableTimer = 0;
            }
            clickDisableTimer += Time.deltaTime;
        }
        else
        {
            //if active check if a bone should be droped
            if(!dragging && Input.GetMouseButtonUp(0)) 
            {
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

                activeBone = null; 
                return; 
            }
            if(dragging && Input.GetMouseButtonUp(1))
            {
                activeBone = null;
                return;
            }
            if (!dragging)
            {
                //rotation
                if (Input.anyKeyDown)
                {
                    const float rotationSpeed = 180;
                    float yAmount = ((Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 :0)) * Time.deltaTime * rotationSpeed;
                    float xAmount = ((Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 :0)) * Time.deltaTime * rotationSpeed;
                    void rotateY(bone toApply, FunctionArgs e)
                    {
                        toApply.transform.RotateAround((e as VecPair).vec1, Vector3.up, (e as VecPair).f);
                    }
                    void rotateX(bone toApply, FunctionArgs e)
                    {
                        toApply.transform.RotateAround((e as VecPair).vec1, Vector3.right, (e as VecPair).f);
                    }
                    if (yAmount != 0)
                        activeBone.Group.applyToAll(rotateY, new VecPair(transform.position, yAmount));
                    if (xAmount != 0)
                        activeBone.Group.applyToAll(rotateX, new VecPair(transform.position, xAmount));
                }

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
                Vector3 worldCord = activeBone.transform.localToWorldMatrix.MultiplyPoint(localDragPoint);
                Vector3 force = (- worldCord + Camera.main.transform.position + Vector3.Project(worldCord - Camera.main.transform.position, fromCamera)).normalized * Time.deltaTime * dragPower;

                activeBone.Rb.AddForceAtPosition(force, worldCord);
            }
        }
    }
}
