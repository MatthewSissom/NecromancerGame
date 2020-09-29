using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionArgs { }
public delegate void GroupFunction(bone applyTo, FunctionArgs e);

public class boneGroup : MonoBehaviour
{
    protected boneGroup parent;
    protected List<boneGroup> children;
    protected bone mBone;
    protected int groupID;
    protected int myID;

    public boneGroup Parent
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
            foreach(boneGroup b in children)
            {
                b.GroupID = value;
            }
        }
    }

    private void Awake()
    {
        children = new List<boneGroup>();
    }

    // Start is called before the first frame update
    virtual protected void Start()
    {
        myID = boneManager.Instance.GetID();

        //only set group id if not on the conveyer
        if (!parent)
            groupID = myID;

        mBone = gameObject.GetComponent<bone>();
    }

    public static void combineGroups(boneGroup one, boneGroup two)
    {
        if (!one.parent)
        {
            two.addChild(one);
            one.resetID();
            boneManager.Instance.SetFate(one,BoneFates.Merged);
        }
        else if (!two.parent)
        {
            one.addChild(two);
            two.resetID();
            boneManager.Instance.SetFate(two, BoneFates.Merged);
        }
        else
        {
            two.makeRoot();
            one.addChild(two);
            two.resetID();
            boneManager.Instance.SetFate(two, BoneFates.Merged);
        }
    }

    public void addChild(boneGroup child)
    {
        children.Add(child);
        child.Parent = this;
    }

    public void removeChild(boneGroup child)
    {
        children.Remove(child);
        child.parent = null;
    }

    public void removeFromConvayer()
    {
        if (parent)
        {
            if (parent.myID != 0)
            {
                parent.removeFromConvayer();
                return;
            }
            parent.removeChild(this);
            resetID();
        }
    }

    public void resetID()
    {
        if (parent) GroupID = parent.groupID;
        else GroupID = myID;
    }

    //makes this bone the root of the tree
    public void makeRoot()
    {
        boneGroup temp = this;
        List<boneGroup> path = new List<boneGroup>();
        while(temp)
        {
            path.Add(temp);
            temp = temp.parent;
        }

        boneGroup child;
        for(int i = path.Count-2; i != -1; --i)
        {
            child = path[i];
            child.addChild(child.parent);
            child.parent.removeChild(child);
            child.resetID();
        }
    }

    public delegate void applyToAllType(GroupFunction func, FunctionArgs e = null, bool rootReached = false);
    public virtual void applyToAll(GroupFunction func, FunctionArgs e = null, bool rootReached = false)
    {
        //traverse up the tree until the root is reached
        if (!rootReached && parent)
        {
            parent.applyToAll(func, e);
        }
        //root reached, recursively call the function on all children
        else
        {
            func(mBone, e);
            foreach (boneGroup b in children)
            {
                b.applyToAll(func, e, true);
            }
        }
    }
}
