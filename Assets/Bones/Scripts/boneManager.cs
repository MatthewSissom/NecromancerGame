using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum BoneFates
{
    None = 0,
    Merged,
    Destroyed
}

public class boneManager : MonoBehaviour
{
    static public boneManager Instance { get; private set; }
    public int numGroups { get; private set; }

    private int currentID = 0;

    //holds an int for the end state of every bone group
    public List<BoneFates> boneFates;

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

        boneFates = new List<BoneFates>();
        boneFates.Add(BoneFates.Merged);
    }

    private void Start()
    {
    }

    public int GetID()
    {
        return currentID++;
    }

    public bone NewBone(GameObject pref, Vector3 location, Quaternion rotation)
    {
        boneFates.Add(BoneFates.None);
        var go = Instantiate(pref, location, rotation);
        return go.GetComponent<bone>();
    }

    public void SetFate(boneGroup b, BoneFates f)
    {
        if (boneFates.Count == b.GroupID)
            Debug.LogError(string.Format("Id :{0} CurrentId: {1} Count {2}", b.GroupID, currentID, boneFates.Count));
        if (f > boneFates[b.GroupID])
            boneFates[b.GroupID] = f;
    }

    public PartialScore ConnectionScore()
    {
        bool oneNone = false;
        foreach(BoneFates bf in boneFates)
        {
            if(bf == BoneFates.None)
            {
                if(oneNone)
                {
                    return new PartialScore("connection", 0, "Skeleton is not connected, + 0\n");
                }
                oneNone = true;
            }
        }
        return new PartialScore("connection", 1000, "Skeleton is fully connected, + 1000!\n");
    }

    public PartialScore LostBones()
    {
        int lostCount = 0;
        foreach (BoneFates bf in boneFates)
        {
            if (bf == BoneFates.Destroyed)
            {
                lostCount++;
            }
        }
        if (lostCount == 0)
        {
            return new PartialScore("lostBone", 1000, "No lost bones, + 1000!");
        }
        return new PartialScore("lostBone", -50 * lostCount, "Lost " + lostCount.ToString() + " bone" + (lostCount > 1 ? "s " : " ") + ", " + (lostCount * -50).ToString());
    }
}