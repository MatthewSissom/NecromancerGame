using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public partial class BoneManager : MonoBehaviour
{/*
    static public BoneManager Instance { get; private set; }
    static public BoneCollisionHandler Collision { get; private set; }


    private HashSet<Bone> activeBones;

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

        activeBones = new HashSet<Bone>();
        Collision = new BoneCollisionHandler();
    }

    private void Start()
    {
        Collision.SetGhostCollision(GhostManager.Collision);
    }

    private void Update()
    {
        Collision.Update(Time.deltaTime);
    }

    public int GetNewGroupID()
    {
        return currentID++;
    }

    public void Register(Bone newBone)
    {
        //avoid duplicates
        if (activeBones.Contains(newBone))
            return;

        if (!newBone.TryGetComponent<BoneGroup>(out BoneGroup group))
        {
            group = newBone.gameObject.AddComponent(typeof(BoneGroup)) as BoneGroup;
           
        }

        GrabbableGroup grabbableGroup = newBone.transform.root.gameObject.GetComponent<GrabbableGroup>();
        if(!grabbableGroup)
        {
            Debug.LogError("Bone is not grabbable, please add a grabbableGroup component");
            return;
        }
        BoneGroup.CombineGroups(grabbableGroup, group);

        activeBones.Add(newBone);
    }

    //removes all refrences to this bone from the manager
    public void Release(Bone toRelease)
    {
        activeBones.Remove(toRelease);
    }

    public GrabbableGroup NewBoneGroup(GameObject pref, GhostBehavior heldBy)
    {
        var go = Instantiate(pref, heldBy.BoneLocation, pref.transform.rotation);
        GrabbableGroup heldGroup = go.GetComponent<GrabbableGroup>();


        heldBy.mBone = heldGroup;
        heldGroup.Rb.freezeRotation = true;
        heldGroup.mGhost = heldBy;
        //set physics layers
        heldGroup.gameObject.layer = 8;
        foreach (Bone b in go.GetComponentsInChildren<Bone>())
        {
            Collision.SetPhysicsLayer(b, 8);
        }

        return heldGroup;
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
        activeBones = new HashSet<Bone>();
    }*/
}