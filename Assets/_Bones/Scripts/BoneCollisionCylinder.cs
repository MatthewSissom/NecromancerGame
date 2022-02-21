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
            myLineRenderer.SetPosition(0, myVertex.transform.position);
            myLineRenderer.SetPosition(1, myBone.currentCylinderHit.MyVertex.transform.position);
        } else
        {
            myLineRenderer.enabled = false;
        }
    }

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
