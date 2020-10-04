using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class for all touches that has no functionality
public class TouchProxy : MonoBehaviour
{
    public float radius { get; set; }

    public delegate void destroyCallback();
    public event destroyCallback DestroyEvent;

    public virtual void Move(Vector3 pos, float rad)
    {
        transform.position = pos;
        radius = rad;
    }

    protected virtual void OnDestroy()
    {
        DestroyEvent?.Invoke();
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
