using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableConnectionArea : BoneGroup
{
    //neighbors who are part of the same chain, used for bone assembly
    [SerializeField]
    public List<TableConnectionArea> chainNeighbors = default;
    //if this area has had it's bones remove for assembly, new axis will be set to the axis of the bone
    //axis will be oriented with the first point being at the origin of the joint the bone belongs to
    public List<Vector3> newAxis = null;

    //maps a boneID to the number of colliders that a bone is touching in this area
    Dictionary<Bone, int> collisionCounts;

    public int GetCollisionCount(Bone bone)
    {
        return collisionCounts[bone];
    }

    public void AddCollision(Bone bone)
    {
        if (!collisionCounts.ContainsKey(bone))
            collisionCounts.Add(bone, 0);
        if (collisionCounts[bone] == 0)
            NewCollision(bone);
        collisionCounts[bone]++;
    }

    public void RemoveCollision(Bone bone)
    {
        collisionCounts[bone]--;
        if (collisionCounts[bone] == 0)
        {
            EndedCollision(bone);
        }
    }

    private void NewCollision(Bone bone)
    {
        BoneManager.Collision.AddTableAreaCollision(bone, this);
    }

    private void EndedCollision(Bone bone)
    {
        BoneManager.Collision.RemoveTableAreaCollision(bone, this);
    }

    protected override void Awake()
    {
        base.Awake();
        void SetVoxelParent(Transform toCheck)
        {
            TableVoxel tv = toCheck.GetComponent<TableVoxel>();
            if (tv) tv.parent = this;

            int children = toCheck.childCount;
            for (int i = 0; i < children; i++)
            {
                SetVoxelParent(toCheck.GetChild(i));
            }
        }
        SetVoxelParent(transform);
    }

    public void ResetArea()
    {
        children = children ?? new List<BoneGroup>();
        while(children.Count != 0)
        {
            BoneGroup child = children[children.Count - 1];
            if (child != null)
            {
                RemoveChild(child);
                Destroy(child.transform.root);
            }
        }
        collisionCounts = new Dictionary<Bone, int>();
    }

    protected override void Start()
    {
        myID = BoneManager.Collision.Regester(this);
        groupID = myID;

        GameManager.Instance.AddEventMethod(typeof(BoneAssembler), "Begin", () => { gameObject.SetActive(false); });
    }
}
