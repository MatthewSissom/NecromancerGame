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
#if (UNITY_EDITOR == false)
        //TODO add case for tutorial
        yield return StartCoroutine(MainGameLoop());
#else
        yield return StartCoroutine(DebugLoops());
#endif
    }

    public IEnumerator MainGameLoop()
    {
        yield return SetState(typeof(GameInit));

        //bone delivery loop
        while (!GhostManager.Instance.done)
        {
            yield return CameraTransition("GhostTrans");
            yield return SetState(typeof(GhostManager));
            yield return CameraTransition("TableTrans");
            CountDown.SetParams("Assemble Cat", GhostManager.Instance.timeBetweenShipments);
            yield return SetState(typeof(CountDown));
        }

        yield return SetState(typeof(BoneAssembler));        
        
        yield return CameraTransition("ToPlayPenMid");
        yield return CameraTransition("PlayPen");
        yield return SetState(typeof(PlayPenState));

        yield return SetState(typeof(GameCleanUp));

        yield break;
    }

    public IEnumerator Tutorial()
    {
        yield return SetState(typeof(GameInit));

        yield return CameraTransition("GhostTrans");
        yield return SetState(typeof(GhostManager));

        MenuManager.Instance.GoToMenu("Main");
        yield return SetState(MenuManager.Instance.InMenus());

        yield return CameraTransition("TableTrans");

        CountDown.SetParams("Assemble Cat", GhostManager.Instance.timeBetweenShipments);
        yield return SetState(typeof(CountDown));

        yield return SetState(typeof(BoneAssembler));

        yield return CameraTransition("ToPlayPenMid");
        yield return CameraTransition("PlayPen");
        yield return SetState(typeof(PlayPenState));

        yield return SetState(typeof(GameCleanUp));

        yield break;
    }

#if UNITY_EDITOR
    public IEnumerator DebugLoops()
    {
        switch (DebugModes.StateMode)
        {
            case DebugModes.EStateDebugMode.PlaypenOnly:
                yield return SetState(typeof(GameInit));

                yield return CameraTransition("ToPlayPenMid");
                yield return new WaitForSeconds(0.2f);
                yield return CameraTransition("PlayPen");

                yield return SetState(typeof(PlayPenState));
                yield return SetState(typeof(GameCleanUp));
                break;
            case DebugModes.EStateDebugMode.Tutorial:
                yield return StartCoroutine(Tutorial());
                break;
            default:
                yield return StartCoroutine(MainGameLoop());
                break;
        }
    }
#endif
}
