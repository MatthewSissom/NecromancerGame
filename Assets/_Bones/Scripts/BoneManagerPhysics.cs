using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoneManager : MonoBehaviour
{
    //holds all the tableConnectionAreas a bone is currently in
    private Dictionary<Bone, LinkedList<TableConnectionArea>> connectionAreaOverlaps;

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

    public void SetPhysicsLayer(Bone toSet, int layer, float delay)
    {
        IEnumerator DelayedSet()
        {
            yield return new WaitForSeconds(delay);
            SetPhysicsLayer(toSet, layer);
            yield break;
        }
        StartCoroutine(DelayedSet());
    }

    private void AttachBoneToTable(Bone toAttach)
    {
        int areaCount = connectionAreaOverlaps[toAttach].Count;
        if (areaCount == 0 || !toAttach){ return; }

        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/BoneConnections");

        ParticleManager.CreateEffect("CombineFX", toAttach.Rb.worldCenterOfMass);

        if (areaCount == 1)
        {
            connectionAreaOverlaps[toAttach].First.Value.ConnectBone(toAttach);
        }
        else
        {
            //find the area with the most overlaps
            int maxCount = 0;
            TableConnectionArea maxCollisions = null;
            foreach(TableConnectionArea cArea in connectionAreaOverlaps[toAttach])
            {
                //reuse area count to hold collision counts
                areaCount = cArea.GetCollisionCount(toAttach);
                if (areaCount > maxCount)
                {
                    maxCount = areaCount;
                    maxCollisions = cArea;
                }
                else if (areaCount > maxCount)
                    Debug.Log("Potential ambiguous result");
            }

            maxCollisions.ConnectBone(toAttach);
        }
    }

    private void AttachToTableTimer(Bone toAttach)
    {
        IEnumerator Timer()
        {
            yield return new WaitForSeconds(0.2f);
            AttachBoneToTable(toAttach);
        }
        StartCoroutine(Timer());
    }

    public void AddTableAreaCollision(Bone bone, TableConnectionArea area)
    {
        //add bone to dictionary tracking overlaps
        if(!connectionAreaOverlaps.ContainsKey(bone))
        {
            connectionAreaOverlaps.Add(bone, new LinkedList<TableConnectionArea>());
        }
        connectionAreaOverlaps[bone].AddLast(area);

        //if this is the only connection, start a timer
        //if there is more than one connection there is already a timer running
        if(connectionAreaOverlaps[bone].Count == 1)
        {
            AttachToTableTimer(bone);
        }
    }

    public void RemoveTableAreaCollision(Bone bone, TableConnectionArea area)
    {
        connectionAreaOverlaps[bone].Remove(area);
    }

    private void PhysicsInit()
    {
        CustomGravity.SetOrigin(Camera.main.transform);
        connectionAreaOverlaps = new Dictionary<Bone, LinkedList<TableConnectionArea>>();
    }
}
