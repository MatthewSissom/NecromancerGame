using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneCollisionCylinder : MonoBehaviour
{
    [SerializeField]
    private Material primaryMat;
    [SerializeField]
    private Material auxiliaryMat;

    private LineRenderer myLineRenderer;

    private bool isParentsActiveCollision;

    private BoneGroup myBone;

    public BoneGroup MyBone
    {
        get
        {
            return myBone;
        }
        set
        {
            myBone = value;
        }
    }

    private GameObject myVertex;

    public GameObject MyVertex
    {
        get
        {
            return myVertex;
        }
        set
        {
            myVertex = value;
        }
    }

    private BoneVertexType myType;

    public BoneVertexType MyType
    {
        get
        {
            return myType;
        }
        set
        {
            myType = value;
        }
    }
    void Awake()
    {
        myLineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isParentsActiveCollision)
        {
            myLineRenderer.enabled = true;
            myLineRenderer.SetPositions(new Vector3[2]);
            myLineRenderer.SetPosition(0, myVertex.transform.position/*new Vector3(myVertex.transform.position.x,
                myBone.currentCylinderHit.MyVertex.transform.position.y, 
                myVertex.transform.position.z)*/);
            myLineRenderer.SetPosition(1, myBone.currentCylinderHit.MyVertex.transform.position);
        } else
        {
            myLineRenderer.enabled = false;
        }
    }

    /*private static Vector3 projToCamera(Vector3 point)
    {
        Vector3 v = point - Camera.main.transform.position;
        float dist = Vector3.Dot(Camera.main.transform.forward, v);
        return (point - Camera.main.transform.forward * (dist - 0.11f));
    }*/

    public void SetPrimary()
    {
        GetComponent<MeshRenderer>().material = primaryMat;
    }

    public void SetAuxiliary()
    {
        GetComponent<MeshRenderer>().material = auxiliaryMat;
    }

    public void OnTriggerEnter(Collider other)
    {
        if(!myBone.isAttached && myBone.isBeingDragged && other.gameObject.tag == "ColliderCylinder")
        {
            if(myBone.currentCylinderHit == null)
            {
                myBone.currentCylinderHit = other.gameObject.GetComponent<BoneCollisionCylinder>();
                isParentsActiveCollision = true;
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (!myBone.isAttached && myBone.isBeingDragged && other.gameObject.tag == "ColliderCylinder")
        {
            if(isParentsActiveCollision)
            {
                myBone.currentCylinderHit = null;
                isParentsActiveCollision = false;
            }
        }
    }
}
