using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Highest level state manager, controls the game flow 
public class StateManager : StateManagerBase
{

    public static StateManager Instance;

    /// <summary>
    /// Main Game Loop Coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator MainLoop()
    {
        yield return SetState(MenuManager.Instance.InMenus());
        while (true)
        {
            yield return SetState(GameManager.Instance.Game());
            yield return SetState(MenuManager.Instance.InMenus());
        }
    }

    override protected void Awake()
    {
        if (Instance)
            Destroy(this);
        else
            Instance = this;

        base.Awake();
    }

    protected override bool IsMyState(State state)
    {
        string name = state.Name;
        return base.IsMyState(state)
           && name.Substring(0, 4) == "StMn";
    }

    // Start is called before the first frame update
    protected void Start()
    {
#if (UNITY_EDITOR == false)
        StartCoroutine(MainLoop());
#else
        StartCoroutine(DebugLoops());
#endif
    }

#if UNITY_EDITOR
    private IEnumerator DebugLoops()
    {
        switch (DebugModes.StateMode)
        {
            default:
                yield return StartCoroutine(MainLoop());
                break;
            case DebugModes.EStateDebugMode.SkipMenus:
                yield return StartCoroutine(NoMenuLoop());
                break;
            case DebugModes.EStateDebugMode.AssemblyAndPlayPen:
                GhostManager.Instance.ShortenDeliveryTimes();
                yield return StartCoroutine(NoMenuLoop());
                break;
            case DebugModes.EStateDebugMode.PlaypenOnly:
                while (true)
                {
                    GameObject testSkeleton = Instantiate(DebugModes.IKTestPrefab);
                    // give time for cat to init
                    yield return new WaitForSeconds(.1f);
                    PlayPenState.Instance.SetSkeleton(testSkeleton);
                    yield return SetState(GameManager.Instance.Game());
                }
        }
    }

    private IEnumerator NoMenuLoop()
    {
        while (true)
        {
            yield return SetState(GameManager.Instance.Game());
        }
    }
#endif
}
