using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformChain
{
    protected List<Transform> orderedTransforms;
    HashSet<Transform> transformSet;
    public GameObject Target { get; private set; }

    protected int firstEmpty;
    bool hasOffset;

    public TransformChain(Transform[] transforms, GameObject target, bool hasOffset)
    {
        //TODO: Transform chains no longer track empties now they're no longer used in assembly
        //      need to add checks to see which transforms are empty

        Target = target;
        this.hasOffset = hasOffset;
        firstEmpty = hasOffset? 1 : 0;
        orderedTransforms = new List<Transform>(transforms);
        transformSet = new HashSet<Transform>(orderedTransforms);
    }

    public virtual bool Contains(Transform toCheck)
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
