using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BoneManager : MonoBehaviour
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
        CustomGravity.SetOrigin(Camera.main.transform);
    }

    public int GetID()
    {
        return currentID++;
    }

    public void SetBoneLayer(Bone toSet,int layer)
    {
        void SetLayerOfAllChildren(Transform t)
        {
            t.gameObject.layer = layer;
            for (int i = 0; i < t.childCount; i++)
            {
                SetLayerOfAllChildren(t.GetChild(i));
            }
        }
        SetLayerOfAllChildren(toSet.transform);
    }

    public void SetBoneLayer(Bone toSet, int layer, float delay)
    {
        IEnumerator DelayedSet()
        {
            yield return new WaitForSeconds(delay);
            SetBoneLayer(toSet, layer);
            yield break;
        }
        StartCoroutine(DelayedSet());
    }

    public Bone NewBone(GameObject pref, Vector3 location, Quaternion rotation, GhostBehavior heldBy = null)
    {
        if(heldBy)
        {
            location = heldBy.boneLocation.position;
        }
        var go = Instantiate(pref, location, rotation);
        Bone bone = go.GetComponent<Bone>();
        activeBones.AddLast(bone);
        if(heldBy)
        {
            heldBy.mBone = bone;
            bone.mGhost = heldBy;
            SetBoneLayer(bone, 8);
        }

        return bone;
    }

    // Will 2/10/2021 - Added parameter for theme, default normal for normal themed bones
    public Bone NewBone(GameObject pref, Vector3 location, Quaternion rotation, GhostBehavior heldBy = null, string theme = "normal")
    {
        if (heldBy)
        {
            location = heldBy.boneLocation.position;
        }
        var go = Instantiate(pref, location, rotation);
        Bone bone = go.GetComponent<Bone>();
        bone.Theme = theme;
        activeBones.AddLast(bone);
        if (heldBy)
        {
            heldBy.mBone = bone;
            bone.mGhost = heldBy;
            SetBoneLayer(bone, 8);
        }

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
        var allBone = GameObject.FindObjectsOfType<Bone>();
        foreach (Bone b in allBone)
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