using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPenState : State
{
    public override IEnumerator Routine()
    {
        Begin();
        End();
        yield return null;
    }
}
