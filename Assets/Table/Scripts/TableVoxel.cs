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
        parent.AddCollision(other.transform.root.GetComponent<Bone>());
    }

    private void OnTriggerExit(Collider other)
    {
        //all tableVoxels are on the boneTrigger layer which only collides with bones
        //so no checks need to be performed
        parent.RemoveCollision(other.transform.root.GetComponent<Bone>());
    }
}
