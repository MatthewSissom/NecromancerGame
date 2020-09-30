using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class boneManager : MonoBehaviour
{
    static public boneManager Instance { get; private set; }

    private LinkedList<bone> activeBones;
    private LinkedList<bone> deactivatedBones;

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

        activeBones = new LinkedList<bone>();
        deactivatedBones = new LinkedList<bone>();
    }

    public int GetID()
    {
        return currentID++;
    }

    public bone NewBone(GameObject pref, Vector3 location, Quaternion rotation)
    {
        var go = Instantiate(pref, location, rotation);
        bone bone = go.GetComponent<bone>();
        activeBones.AddLast(bone);
        return bone;
    }


    public void DeactivateBone(bone toDeactivate)
    {
        if (toDeactivate && !deactivatedBones.Contains(toDeactivate))
        {
            activeBones.Remove(toDeactivate);
            deactivatedBones.AddLast(toDeactivate);
            toDeactivate.enabled = false;
        }
    }

    public void DestroyBone(bone toDestroy)
    {
        activeBones.Remove(toDestroy);
        Destroy(toDestroy.transform.root.gameObject);
    }

    public PartialScore ConnectionScore()
    {
        //search for any ids that aren't the same 
        int id = (activeBones.Count > 0) ? activeBones.First.Value.Group.GroupID : -1;
        foreach(bone b in activeBones)
        {
            if(b.Group.GroupID != id)
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