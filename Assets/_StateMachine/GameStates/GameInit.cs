using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInit : State
{
    public override IEnumerator Routine()
    {
        Begin();

        GhostManager.Instance.ManagerInit();

        End();
        yield break;
    }
}
