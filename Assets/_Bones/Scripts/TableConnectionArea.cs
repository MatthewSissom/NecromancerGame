using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableConnectionArea : MonoBehaviour
{
    //maps a boneID to the number of colliders that a bone is touching in this area
    Dictionary<Bone, int> collisionCounts;
    BoneGroup group;

    public int GetCollisionCount(Bone bone)
    {
        return collisionCounts[bone];
    }

    public void ConnectBone(Bone bone)
    {
        bone.Rb.isKinematic = true;
        bone.connecting = true;
        
        BoneGroup.CombineGroups(group, bone.Group);
    }

    public void AddCollision(Bone bone)
    {
        if (!collisionCounts.ContainsKey(bone))
            collisionCounts.Add(bone, 0);
        if(collisionCounts[bone] == 0)
            NewCollision(bone);
        collisionCounts[bone]++;
    }

    public void RemoveCollision(Bone bone)
    {
        collisionCounts[bone]--;
        if(collisionCounts[bone] == 0)
        {
            EndedCollision(bone);
        }
    }

    private void NewCollision(Bone bone)
    {
        BoneManager.Instance.AddTableAreaCollision(bone, this);
    }

    private void EndedCollision(Bone bone)
    {
        BoneManager.Instance.RemoveTableAreaCollision(bone, this);
    }

    protected void Awake()
    {
        collisionCounts = new Dictionary<Bone, int>();

        group = gameObject.GetComponent<BoneGroup>();
        if (!group) group = (BoneGroup)gameObject.AddComponent(typeof(BoneGroup));

        void SetVoxelParent(Transform toCheck)
        {
            TableVoxel tv = toCheck.GetComponent<TableVoxel>();
            if (tv) tv.parent = this;

            int children = toCheck.childCount;
            for(int i = 0; i < children; i++)
            {
                SetVoxelParent(toCheck.GetChild(i));
            }
        }
        SetVoxelParent(transform);
    }
}
