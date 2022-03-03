using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionArgs { }
public delegate void GroupFunction(Bone applyTo, FunctionArgs e);

public class BoneGroup : MonoBehaviour
{
    protected BoneGroup parent;
    protected List<BoneGroup> children;
    protected Bone mBone;
    protected int groupID;
    protected int myID;


    public BoneGroup Parent
    {
        set 
        {
            if (parent)
            {
                parent.removeChild(this);
            }
            parent = value;
        }
    }

    public int GroupID { 
        get { return groupID; } 
        set
        {
            groupID = value;
            foreach(BoneGroup b in children)
            {
                b.GroupID = value;
            }
        }
    }

    protected virtual void Awake()
    {
        children = new List<BoneGroup>();
        if (BoneManager.Instance)
        {
            myID = BoneManager.Instance.GetNewGroupID();
            groupID = myID;
        }
        else
            myID = -1;
        mBone = gameObject.GetComponent<Bone>();
    }

    // Start is called before the first frame update
    virtual protected void Start()
    {
        if(myID == -1)
        {
            myID = BoneManager.Instance.GetNewGroupID();
            groupID = myID;
        }
    }

    //Combines two groups in to a single group
    //prefered parent is usually made the parent of the tree, unless it would be expensive to do so.
    //if making the prefered group node the parent is important set preferedMustBeParent to true
    public static void CombineGroups(BoneGroup preferedParent, BoneGroup other, bool preferedMustBeParent = false)
    {
        //check to see if groups are already the same
        if (preferedParent.groupID == other.groupID)
            return;
        //if other has no parent make prefered the parent
        if (!other.parent)
        {
            preferedParent.addChild(other);
            other.ResetID();
        }
        //make other the parent if possible to avoid rearanging trees
        else if (!preferedParent.parent && !preferedMustBeParent)
        {
            Debug.Log("Non ideal parenting");
            other.addChild(preferedParent);
            preferedParent.ResetID();
        }
        //if needed rearange the tree of other so that prefered can be it's parent
        else
        {
            other.makeRoot();
            preferedParent.addChild(other);
            other.ResetID();
        }
    }


    public delegate void applyToAllType(GroupFunction func, FunctionArgs e = null);
    //calls the passeed function on each member of the bone group, passing in the bone and the given function args as peramiters
    public virtual void ApplyToAll(GroupFunction func, FunctionArgs e = null)
    {
        void ApplyFuncRecursive(BoneGroup toApply)
        {
            //TEMP shouldn't need null check, result of tableConArea
            if (toApply.mBone)
                func(toApply.mBone, e);
            foreach (BoneGroup bg in toApply.children)
            {
                ApplyFuncRecursive(bg);
            }
        }

        //start at group root and filer down to all children
        ApplyFuncRecursive(GetRoot());
    }

    //returns the root of this tree
    public BoneGroup GetRoot()
    {
        if (parent)
            return parent.GetRoot();
        return this;
    }

    //gets a list of all bone components in this tree
    public List<Bone> GetAllBones()
    {
        List<Bone> allBones = new List<Bone>();

        void GetBoneRecursive(BoneGroup toCheck)
        {
            if (toCheck.mBone)
                allBones.Add(toCheck.mBone);
            foreach(BoneGroup bg in toCheck.children)
            {
                GetBoneRecursive(bg);
            }
        }

        GetBoneRecursive(GetRoot());
        return allBones;
    }

    protected virtual void RemoveChild(BoneGroup toRemove)
    {
        removeChild(toRemove);
        toRemove.ResetID();
    }

    #region Helpers


    //changes the id of this node to match it's parent
    private void ResetID()
    {
        if (parent) GroupID = parent.groupID;
        else GroupID = myID;
    }

    //adds a child to this group
    private void addChild(BoneGroup child)
    {
        children.Add(child);
        child.Parent = this;
    }

    //removes a child from this group
    private void removeChild(BoneGroup child)
    {
        children.Remove(child);
        child.parent = null;
    }

    //makes this bone the root of the tree
    public void makeRoot()
    {
        BoneGroup temp = this;
        //find the path from this node to the root
        List<BoneGroup> path = new List<BoneGroup>();
        while (temp)
        {
            path.Add(temp);
            temp = temp.parent;
        }

        //swap nodes along the path until this node is the root
        BoneGroup child;
        for (int i = path.Count - 2; i != -1; --i)
        {
            child = path[i];
            child.addChild(child.parent);
            child.parent.removeChild(child);
            child.ResetID();
        }
    }

    #endregion
}
