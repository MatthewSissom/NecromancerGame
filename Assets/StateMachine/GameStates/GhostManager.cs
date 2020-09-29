﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//State in the main game loop, brings bones to the table
public class GhostManager : State
{
    public bool done { get; private set; }
    static public GhostManager Instance;

    [Header("Bones")]
    public List<GameObject> boneShipments;
    private int currentShipment = 0;

    [Header("Ghosts")]
    public GameObject ghostPref;


    //instantiates the bones given 
    public void CreateBones(in List<GameObject> bones)
    {
        Vector3 spacing = new Vector3(-1, 0, 0);
        Vector3 pos = transform.position;


        //instantiate bones
        foreach(GameObject pref in bones)
        {
            Vector3 target = pos + new Vector3(0, 0, -2.2f);
            bone currentBone = boneManager.Instance.NewBone(pref, new Vector3(0,100,0), pref.transform.rotation);
            if (currentBone)
            {
                GhostBehavior ghost = Instantiate(ghostPref, pos, Quaternion.Euler(0,90,0)).GetComponent<GhostBehavior>();
                ghost.mBone = currentBone;
                currentBone.mGhost = ghost;
                ghost.destination = target;
                ghost.timeToDest = 1 + Random.value;
            }
            pos += spacing;
        }
    }

    private float getLength(GameObject bone)
    {
        return bone.GetComponent<Renderer>().bounds.size.x;
    }

    private IEnumerator BoneShipment()
    {
        CreateBones(boneShipments[currentShipment].GetComponent<BoneShipment>().bones);
        done = ++currentShipment == boneShipments.Count;
        yield return new WaitForSeconds(5.0f);
        yield break;
    }

    public override IEnumerator Routine()
    {
        Begin();

        yield return BoneShipment();

        End();
        yield break;
    }

    override protected void Awake()
    {
        base.Awake();
        if(Instance)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
}



