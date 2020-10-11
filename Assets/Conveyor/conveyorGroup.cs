using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class conveyorGroup : BoneGroup
{
    public override void applyToAll(GroupFunction func, FunctionArgs e, bool rootReached = false)
    {
        foreach (BoneGroup b in children)
        {
            b.applyToAll(func, e, true);
        }
    }

    protected override void Start()
    {
        base.Start();
        myID = 0;
    }

    public int groupCount ()
    {
        return children.Count;
    }
}
