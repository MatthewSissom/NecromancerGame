using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCleanUp : State
{
    public override IEnumerator Routine()
    {
        Begin();

        InputManager.Instance.clear();
        InputManager.Instance.enabled = false;
        boneManager.Instance.DestroyAll();

        End();

        yield break;
    }
}
