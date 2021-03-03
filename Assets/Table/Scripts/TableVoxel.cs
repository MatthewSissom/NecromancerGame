using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableVoxel : MonoBehaviour
{
    public TableConnectionArea parent { private get; set; }

    private void OnTriggerEnter(Collider other)
    {
        //all tableVoxels are on the boneTrigger layer which only collides with bones
        //so no checks need to be performed
        var group = other.transform.root.GetComponent<GrabbableGroup>();
        if (!group)
            return;
        Bone temp = group.BoneFromCollider(other);
        if(temp)
            parent.AddCollision(temp);
    }

    private void OnTriggerExit(Collider other)
    {
        //all tableVoxels are on the boneTrigger layer which only collides with bones
        //so no checks need to be performed
        Bone temp = other.transform.root.GetComponent<Bone>();
        if (temp)
            parent.RemoveCollision(temp);
    }
}
