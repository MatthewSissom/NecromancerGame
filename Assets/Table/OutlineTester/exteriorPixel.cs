using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interiorPixel : rayPixel
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        activeColor = new Color(0, .8f, 0);
        inactiveColor = new Color(0, .5f, 0);
    }

    public override int Refresh()
    {
        if(base.Refresh() != -1)
        return 1;
        return -1;
    }
}
