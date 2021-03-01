using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneCollisionHandler
{
    //holds all the other bones a bone is colliding with
    private Dictionary<Bone, LinkedList<BoneGroup>> collisions;
    //provides an effecient way to find a connectionArea given a group id
    private Dictionary<int, TableConnectionArea> groupIdToConnectionArea;

    //used to track timers
    private class ConnectionTimers
    {
        Dictionary<Bone, float> timers;
        LinkedList<Bone> timedBones;
        //increasing timer showing how much time as passed
        float time;

        public ConnectionTimers()
        {
            timers = new Dictionary<Bone, float>();
            timedBones = new LinkedList<Bone>();
        }

        public bool Contains(Bone toCheck)
        {
            foreach (Bone b in timedBones)
                if (b == toCheck)
                    return true;
            return false;
        }

        public void Add(Bone toAdd, float duration = 0.2f)
        {
            //check to see if a timer is running
            bool entryExists = timers.TryGetValue(toAdd, out float timerValue);
            if (entryExists && timerValue != -1)
                return;

            //set timer with value relitive to main timer
            duration += time;
            if (!entryExists)
                timers.Add(toAdd, duration);
            else
                timers[toAdd] = duration;

            //no sorting needed, early return
            if (timedBones.Count == 0)
            {
                timedBones.AddFirst(toAdd);
                return;
            }

            //find the timer's place in the list
            var iterator = timedBones.First;
            while(timers[iterator.Value] < duration)
            {
                iterator = iterator.Next;
                if(iterator == null)
                {
                    timedBones.AddLast(toAdd);
                    return;
                }
            }

            //add timer to list
            timedBones.AddBefore(iterator, toAdd);
        }

        public Bone Update(float elapsedTime)
        {
            if (timedBones.Count == 0)
                return null;

            time += elapsedTime;
            var toCheck = timedBones.First.Value;
            if (time > timers[toCheck])
            {
                timedBones.RemoveFirst();
                timers[toCheck] = -1;
                //reset timer if possible
                if (timedBones.Count == 0)
                    time = 0;
                return toCheck;
            }

            return null;
        }
    }

    private ConnectionTimers connectionTimers;

    public BoneCollisionHandler()
    {
        CustomGravity.SetOrigin(Camera.main.transform);
        collisions = new Dictionary<Bone, LinkedList<BoneGroup>>();
        groupIdToConnectionArea = new Dictionary<int, TableConnectionArea>();
        connectionTimers = new ConnectionTimers();
    }

    public void Update(float deltaTime)
    {
        Bone toCheck = connectionTimers.Update(deltaTime);
        if (toCheck == null)
            return;

        TableConnectionArea toAttachTo = CheckConnectionAreaOverlap(toCheck);
        if (toAttachTo == null)
            return;

        AttachBoneToConnectionArea(toCheck, toAttachTo);
    }

    #region interface

    //---Bones---//

    public void AddBoneCollision(Bone bone, Collision collision)
    {
        //---Bone on bone---//
        if (collision.gameObject.CompareTag("Bone"))
        {
            Bone other = collision.gameObject.GetComponent<Bone>();
            //only process collisions from lower id groups to avoid double processing
            if (!other || bone.Group.GroupID > other.Group.GroupID)
                return;

            //audio
            AudioManager.Instance.PlaySound("normal");

            //connection timers

            //both groups are either alread in table groups and should not connect
            //or cannot connect because neither is connecting
            if (bone.connecting == other.connecting)
                return;

            if (bone.connecting)
                AddCollisionWithGroup(other, bone.Group);
            else if (other.connecting)
                AddCollisionWithGroup(bone, other.Group);
        }

        //---Bone on horizontal surface---//
        else if (collision.gameObject.CompareTag("Horizontal"))
        {
            bone.Rb.velocity = new Vector3(0, bone.Rb.velocity.y, 0);
            bone.Rb.gameObject.GetComponent<CustomGravity>().enabled = false;
            bone.Rb.useGravity = true;
        }
    }

    public void RemoveBoneCollision(Bone bone1, Bone bone2)
    {
        //both groups are either alread in table groups and should not connect
        //or cannot connect because neither is connecting
        if (bone1.connecting == bone2.connecting)
            return;

        if (bone1.connecting)
            RemoveCollisionWithGroup(bone2, bone1.Group);
        else if (bone2.connecting)
            RemoveCollisionWithGroup(bone1, bone2.Group);
    }

    //---TableAreas---//

    //regesters a table connection area with the collision handler so 
    //important values can be cashed
    public int Regester(TableConnectionArea toRegester)
    {
        int id = BoneManager.Instance.GetNewGroupID();
        groupIdToConnectionArea.Add(id, toRegester);
        return id;
    }

    //processes a new collision between a table connection area and a bone
    public void AddTableAreaCollision(Bone bone, TableConnectionArea area)
    {
        AddCollisionWithGroup(bone, area);
    }

    //processes an ended collision between a table connection area and a bone
    public void RemoveTableAreaCollision(Bone bone, TableConnectionArea area)
    {
        RemoveCollisionWithGroup(bone, area);
    }

    //---Layers---//

    //changes the physics layer of a bone
    public void SetPhysicsLayer(Bone toSet, int layer)
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

    #endregion

    #region helpers

    private void RemoveCollisionWithGroup(Bone bone, BoneGroup group)
    {
        if(collisions.ContainsKey(bone))
            collisions[bone].Remove(group);
    }

    private void AddCollisionWithGroup(Bone bone, BoneGroup group)
    {
        //add bone to dictionary tracking overlaps
        if (!collisions.ContainsKey(bone))
        {
            collisions.Add(bone, new LinkedList<BoneGroup>());
        }
        collisions[bone].AddLast(group);

        connectionTimers.Add(bone);
    }

    private bool AttachBoneToConnectionArea(Bone attaching, TableConnectionArea toAttachTo)
    {
        //argument validation
        if (!toAttachTo.enabled)
            return false;

        //physics
        attaching.Rb.isKinematic = true;
        attaching.connecting = true;

        //managers
        BoneManager.Instance.Release(attaching);
        BoneGroup.CombineGroups(toAttachTo, attaching.Group, true);

        //visuals
        var gatherer = attaching.gameObject.GetComponent<RendererGatherer>();
        if (gatherer) gatherer.ChangeMat();
        ParticleManager.CreateEffect("CombineFX", attaching.Rb.worldCenterOfMass);

        //audio
        //FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/BoneConnections");

        return true;
    }

    //returns the area with the greatest score
    //score is calculated by first finding which areas on the table the bone overlaps with, which has
    //by far the most impact on connection
    //collisions with other bones are used to break ties, with areas that DONT have the colliding bone
    //in them winning out. this makes it more likely that the skeleton will have more bones in more 
    //areas of the skeleton
    private TableConnectionArea CheckConnectionAreaOverlap(Bone toCheck)
    {
        int areaCount = collisions[toCheck].Count;

        //early returns for trivial cases
        if (areaCount == 0)
            return null;
        if (areaCount == 1)
        {
            return groupIdToConnectionArea[collisions[toCheck].First.Value.GroupID];
        }

        //calculate scores
        Dictionary<int, float> idToScore = new Dictionary<int, float>();
        foreach (BoneGroup bGroup in collisions[toCheck])
        {
            int id = bGroup.GroupID;
            if(!idToScore.ContainsKey(id))
                idToScore.Add(id,0);

            //check for group type
            TableConnectionArea area = bGroup as TableConnectionArea;
            if (area != null)
            {
                idToScore[id] += area.GetCollisionCount(toCheck);
            }
            else
            {
                //subtract a small number for tie breaking purposes
                idToScore[id] -= 0.1f;
            }
        }

        //search for a positive score first
        float maxScore = 0;
        int maxId = 0;
        foreach(int id in idToScore.Keys)
        {
            if (idToScore[id] > maxScore)
            {
                maxScore = idToScore[id];
                maxId = id;
            }
        }
        if (maxScore > 0)
            return groupIdToConnectionArea[maxId];

        //if no positive scores are found search for the lowest score
        foreach (int id in idToScore.Keys)
        {
            if (idToScore[id] < maxScore)
            {
                maxScore = idToScore[id];
                maxId = id;
            }
        }

        if (maxScore < 0)
            return groupIdToConnectionArea[maxId];

        Debug.LogError("Collision handler failed to resolve complex collision problem");
        return null;
    }

    #endregion
}
