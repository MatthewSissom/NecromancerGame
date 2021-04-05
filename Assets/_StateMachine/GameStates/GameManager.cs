using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : StateManagerBase
{
    public static GameManager Instance;

    protected override void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        else
            Instance = this;

        base.Awake();
    }

    protected override bool IsMyState(State state)
    {
        string name = state.Name;
        string managerKey = name.Substring(0, 4);
        return base.IsMyState(state)
            &&(managerKey != "StMn" && managerKey != "Menu");
    }

    public IEnumerator Game()
    {
        yield return SetState(typeof(GameInit));

        //bone delivery loop
        while (!GhostManager.Instance.done)
        {
            yield return CameraTransition("GhostTrans");
            yield return SetState(typeof(GhostManager));
            yield return CameraTransition("TableTrans");
            yield return new WaitForSeconds(GhostManager.Instance.timeBetweenShipments);
        }


        yield return SetState(typeof(BoneAssembler));
        yield return SetState(typeof(AssignmentChecker));

        yield return CameraTransition("PlayPen");


        while(true)
        {
            yield return null;
        }

        yield return SetState(typeof(GameCleanUp));

        yield break;
    }
}
