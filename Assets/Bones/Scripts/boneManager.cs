using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class boneManager : MonoBehaviour
{
    static public boneManager Instance { get; private set; }
    public int numGroups { get; private set; }

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
        var go = Instantiate(pref, location, rotation);
        return go.GetComponent<bone>();
    }


    public PartialScore ConnectionScore()
    {
        return new PartialScore("connection", 0, "Skeleton is not connected, + 0\n");

        return new PartialScore("connection", 1000, "Skeleton is fully connected, + 1000!\n");
    }

    public PartialScore LostBones()
    {
        int lostCount = 0;
        if (lostCount == 0)
        {
            return new PartialScore("lostBone", 1000, "No lost bones, + 1000!");
        }
        return new PartialScore("lostBone", -50 * lostCount, "Lost " + lostCount.ToString() + " bone" + (lostCount > 1 ? "s " : " ") + ", " + (lostCount * -50).ToString());
    }
}