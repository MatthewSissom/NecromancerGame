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

    private void Awake()
    {
        children = new List<BoneGroup>();
    }

    // Start is called before the first frame update
    virtual protected void Start()
    {
        myID = BoneManager.Instance.GetID();

        //only set group id if not on the conveyer
        if (!parent)
            groupID = myID;

        mBone = gameObject.GetComponent<Bone>();
    }

    public static void combineGroups(BoneGroup one, BoneGroup two)
    {
        if (!one.parent)
        {
            two.addChild(one);
            one.resetID();
        }
        else if (!two.parent)
        {
            one.addChild(two);
            two.resetID();
        }
        else
        {
            two.makeRoot();
            one.addChild(two);
            two.resetID();
        }
    }

    public void addChild(BoneGroup child)
    {
        children.Add(child);
        child.Parent = this;
    }

    public void removeChild(BoneGroup child)
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
        BoneGroup temp = this;
        List<BoneGroup> path = new List<BoneGroup>();
        while(temp)
        {
            path.Add(temp);
            temp = temp.parent;
        }

        BoneGroup child;
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
            foreach (BoneGroup b in children)
            {
                b.applyToAll(func, e, true);
            }
        }
    }
}
