using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public partial class BoneManager : MonoBehaviour
{
    static public BoneManager Instance { get; private set; }

    private LinkedList<Bone> activeBones;
    private LinkedList<Bone> deactivatedBones;

    private int currentID = 0;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }
        else
        {
            Instance = this;
        }

        activeBones = new LinkedList<Bone>();
        deactivatedBones = new LinkedList<Bone>();

        PhysicsInit();
    }

    public int GetNewID()
    {
        return currentID++;
    }

    public void Register(Bone newBone)
    {
        if(!newBone.GetComponent<BoneGroup>())
            newBone.gameObject.AddComponent(typeof(BoneGroup));
        activeBones.AddLast(newBone);
    }

    //removes all refrences to this bone from the manager
    public void Release(Bone toRelease)
    {
        activeBones.Remove(toRelease);
        deactivatedBones.Remove(toRelease);
    }

    public Bone NewBone(GameObject pref, GhostBehavior heldBy)
    {
        var go = Instantiate(pref, heldBy.boneLocation.position, pref.transform.rotation);
        Bone bone = go.GetComponent<Bone>();

        heldBy.mBone = bone;
        bone.mGhost = heldBy;
        SetPhysicsLayer(bone, 8);
        bone.Rb.freezeRotation = true;

        return bone;
    }

    public void DeactivateBone(Bone toDeactivate)
    {
        //if (toDeactivate && !deactivatedBones.Contains(toDeactivate))
        //{
        //    activeBones.Remove(toDeactivate);
        //    deactivatedBones.AddLast(toDeactivate);
        //    toDeactivate.enabled = false;
        //}
    }

    public void DestroyBone(Bone toDestroy)
    {
        activeBones.Remove(toDestroy);
        Destroy(toDestroy.transform.root.gameObject);
    }

    public void DestroyAll()
    {
        foreach (Bone b in activeBones)
        {
            Destroy(b.transform.root.gameObject);
        }
        foreach (Bone b in deactivatedBones)
        {
            Destroy(b.transform.root.gameObject);
        }
        activeBones = new LinkedList<Bone>();
        deactivatedBones = new LinkedList<Bone>();
    }

    public PartialScore ConnectionScore()
    {
        //search for any ids that aren't the same 
        int id = (activeBones.Count > 0) ? activeBones.First.Value.Group.GroupID : -1;
        foreach (Bone b in activeBones)
        {
            if (b.Group.GroupID != id)
            {
                id = -1;
                break;
            }
        }

        if (id == -1)
        {
            return new PartialScore("connection", 0, "Skeleton is not connected, + 0\n");
        }
        {
            return new PartialScore("connection", 1000, "Skeleton is fully connected, + 1000!\n");
        }
    }

    public PartialScore LostBones()
    {
        int lostCount = deactivatedBones.Count;
        if (lostCount == 0)
        {
            return new PartialScore("lostBone", 1000, "No lost bones, + 1000!");
        }
        return new PartialScore("lostBone", -50 * lostCount, "Lost " + lostCount.ToString() + " bone" + (lostCount > 1 ? "s " : " ") + ", " + (lostCount * -50).ToString());
    }
}