using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Highest level state manager, controls the game flow 
public class StateManager : StateManagerBase
{
#if UNITY_EDITOR
    public enum EDebugMode { 
        None,
        SkipMenus,
        PlaypenOnly
    }
    [SerializeField]
    EDebugMode debugMode;
    public EDebugMode DebugMode { get { return debugMode; }}
#endif
    public static StateManager Instance;

    /// <summary>
    /// Main Game Loop Coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator MainLoop()
    {
#if UNITY_EDITOR
        switch (DebugMode)
        {
            default:
#endif
                MenuManager.Instance.GoToMenu("Main");
                yield return SetState(MenuManager.Instance.InMenus());
                while (true)
                {
                    yield return SetState(GameManager.Instance.Game());
                    MenuManager.Instance.GoToMenu("Main");
                    yield return SetState(MenuManager.Instance.InMenus());
                }

#if UNITY_EDITOR
                break;
            case EDebugMode.SkipMenus:
                while (true)
                {
                    yield return SetState(GameManager.Instance.Game());
                }
                break;
            case EDebugMode.PlaypenOnly:
                break;
        }
#endif
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
        StartCoroutine(MainLoop());
    }
}
