﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformChain
{
    List<Transform> orderedTransforms;
    HashSet<Transform> transformSet;
    public GameObject Target { get; private set; }

    int firstEmpty;
    bool hasOffset;

    public TransformChain(Transform[] transforms, GameObject target, bool hasOffset)
    {
        Target = target;
        this.hasOffset = hasOffset;
        firstEmpty = hasOffset? 1 : 0;
        orderedTransforms = new List<Transform>(transforms);
        transformSet = new HashSet<Transform>(orderedTransforms);
    }

    public bool Contains(Transform toCheck)
    {
        return transformSet.Contains(toCheck);
    }

    public int Count()
    {
        return orderedTransforms.Count - (hasOffset ? 1 : 0);
    }

    public float WorldLength()
    {
        float length = 0;
        for(int i = 1; i < firstEmpty; i++)
        {
            length += (orderedTransforms[i].position - orderedTransforms[i - 1].position).magnitude;
        }
        return length;
    }

    public List<Transform> GetList()
    {
        return orderedTransforms;
    }

    public Transform FirstEmpty()
    {
        return orderedTransforms[firstEmpty];
    }

    //returns an empty transform and 
    public Transform MoveToFirstEmpty(Transform originalDestination)
    {
        //check to see if a swap was made
        if (originalDestination != orderedTransforms[firstEmpty])
            MoveChildren(originalDestination, orderedTransforms[firstEmpty]);
        return orderedTransforms[firstEmpty++];
    }

    //leaves only one empty target at the end of the chain
    public void DestroyAdditionalEmpties()
    {
        for (int i = orderedTransforms.Count - 1; i > firstEmpty; i--)
        {
            RemoveTransformFromHigharchy(orderedTransforms[i]);
            orderedTransforms.RemoveAt(i);
        }
    }
    public void MoveChildren(Transform removeChildrenFrom, Transform moveChildrenTo)
    {
        if (!Contains(removeChildrenFrom) || !Contains(moveChildrenTo))
            Debug.LogError("Transform chain does not contain the given transforms");

        Vector3 toParent = moveChildrenTo.position - removeChildrenFrom.position;
        while (removeChildrenFrom.childCount > 0)
        {
            var child = removeChildrenFrom.GetChild(0);
            child.parent = moveChildrenTo;
            child.position += toParent;
        }
    }

    void RemoveTransformFromHigharchy(Transform toRemove)
    {
        var parent = toRemove.parent;
        //dont' remove the root
        if (parent == null || parent.parent == null)
            return;

        MoveChildren(toRemove,parent);

        GameObject.Destroy(toRemove.gameObject);
    }

}
