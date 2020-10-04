using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInit : State
{
    public override IEnumerator Routine()
    {
        Begin();

        InputManager.Instance.enabled = true;

        End();
        yield break;
    }
}
