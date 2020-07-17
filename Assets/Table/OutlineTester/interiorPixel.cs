using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class exteriorPixel : rayPixel
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        activeColor = new Color(.6f, .6f, .6f);
        inactiveColor = new Color(.3f, .3f, .3f);
    }

    public override int Refresh()
    {
        if (base.Refresh() != -1)
            return 0;
        return -1;
    }
}
