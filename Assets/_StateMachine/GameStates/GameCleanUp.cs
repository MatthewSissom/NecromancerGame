﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCleanUp : State
{
    public override IEnumerator Routine()
    {
        Begin();

        InputManager.Instance.Clear();
        InputManager.Instance.enabled = false;
        GhostManager.Instance.DestroyAll();
        GhostManager.Instance.TutorialPhaseFinished = null;
        //BoneManager.Instance.DestroyAll();

        End();

        yield break;
    }
}
