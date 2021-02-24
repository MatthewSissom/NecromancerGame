using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCatState : State
{
    public override IEnumerator Routine()
    {
        transform.position = new Vector3(-4, -.5f, -4);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.parent = null;
        GetComponent<TestWalk>().enabled = true;
        yield break;
    }
}
