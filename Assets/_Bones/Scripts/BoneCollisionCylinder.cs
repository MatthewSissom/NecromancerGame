using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneCollisionCylinder : MonoBehaviour
{
    [SerializeField]
    private Material primaryMat;
    [SerializeField]
    private Material auxiliaryMat;

    private MeshRenderer indicatorRenderer;
    private LineRenderer myLineRenderer;

    private bool isParentsActiveCollision;

    public bool IsParentsActiveCollision
    {
        get
        {
            return isParentsActiveCollision;
        }
        set
        {
            isParentsActiveCollision = value;
            if(isParentsActiveCollision)
            {
                indicatorRenderer.material = auxiliaryMat;
            } else
            {
                indicatorRenderer.material = primaryMat;
            }
        }
    }


    private bool isChildsActiveCollision;

    private bool IsChildsActiveCollision
    {
        get
        {
            return isChildsActiveCollision;
        }
        set
        {
            isChildsActiveCollision = value;
            if (isChildsActiveCollision)
            {
                indicatorRenderer.material = auxiliaryMat;
            }
            else
            {
                indicatorRenderer.material = primaryMat;
            }
        }
    }

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

    private GameObject myIndicator;

    public GameObject MyIndicator
    {
        get
        {
            return myIndicator;
        }
        set
        {
            myIndicator = value;
            indicatorRenderer = MyIndicator.GetComponent<MeshRenderer>();
            indicatorRenderer.material = primaryMat;
        }
    }
    void Awake()
    {
        myLineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsParentsActiveCollision)
        {
            if(myBone.currentCylinderHit)
            {
                myLineRenderer.enabled = true;
                myLineRenderer.SetPositions(new Vector3[2]);
                myLineRenderer.SetPosition(0, myVertex.transform.position/*new Vector3(myVertex.transform.position.x,
                myBone.currentCylinderHit.MyVertex.transform.position.y, 
                myVertex.transform.position.z)*/);
                myLineRenderer.SetPosition(1, myBone.currentCylinderHit.MyVertex.transform.position);
            }
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

    public void SetVisible()
    {
        indicatorRenderer.enabled = true;
    }

    public void SetInvisible()
    {
        indicatorRenderer.enabled = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if(!myBone.isAttached && myBone.isBeingDragged && other.gameObject.tag == "ColliderCylinder")
        {
            if(myBone.currentCylinderHit == null || myBone.currentCylinderDoingHitting != this)
            {
                if(myBone.currentCylinderDoingHitting)
                {
                    myBone.currentCylinderDoingHitting.IsParentsActiveCollision = false;
                }
                if(myBone.currentCylinderHit)
                {
                    myBone.currentCylinderHit.IsChildsActiveCollision = false;
                }

                BoneCollisionCylinder otherCyl = other.gameObject.GetComponent<BoneCollisionCylinder>();
                myBone.currentCylinderHit = otherCyl;
                otherCyl.IsChildsActiveCollision = true;

                myBone.currentCylinderDoingHitting = this;
                IsParentsActiveCollision = true;
                myBone.currentCollisionVertex = MyType;
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (!myBone.isAttached && myBone.isBeingDragged && other.gameObject.tag == "ColliderCylinder")
        {
            if(IsParentsActiveCollision)
            {
                myBone.currentCylinderHit = null;
                myBone.currentCylinderDoingHitting = null;
                IsParentsActiveCollision = false;
                myBone.currentCollisionVertex = null;
            }
            other.gameObject.GetComponent<BoneCollisionCylinder>().IsChildsActiveCollision = false;
        }
    }
}
