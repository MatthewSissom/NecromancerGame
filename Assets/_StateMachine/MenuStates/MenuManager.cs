using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : StateManagerBase
{
    public static MenuManager Instance;
    public string state;

    public IEnumerator InMenus()
    {
        CameraTransition("MenuBoardFlipped");
        yield return CameraTransition("MenuTransMain");
        yield return SetState(typeof(MenuIntroScreen));
        while (true)
        {
            switch (state)
            {
                case "Main":
                    state = null;
                    yield return CameraTransition("MenuTransMain");
                    yield return CameraTransition("MenuBoardMain");
                    yield return SetState(typeof(MenuMain));
                    break;
                case "Play":
                    state = null;
                    yield break;
                case "Instructions":
                    state = null;
                    yield return CameraTransition("MenuBoardFlipped");
                    yield return SetState(typeof(MenuInstructions));
                    break;
                case "Score":
                    state = null;
                    yield return CameraTransition("MenuTransMain");
                    yield return CameraTransition("MenuBoardFlipped");
                    yield return SetState(typeof(MenuDisplayScore));
                    break;
                // Will - used for displaying assignments
                case "Assignment":
                    state = null;
                    yield return CameraTransition("MenuBoardFlipped");
                    yield return SetState(typeof(MenuShowAssignments));
                    break;
                // Will - used for displaying assignments
                case "Options":
                    state = null;
                    yield return CameraTransition("MenuBoardFlipped");
                    yield return SetState(typeof(MenuMusicSliders));
                    break;
                case null:
                    yield return null;
                    break;
            }
        }
    }

    public void GoToMenu(string name)
    {
        state = name;
    }

    protected override void Awake()
    {
        if (Instance)
            Destroy(gameObject);
        else
            Instance = this;

        base.Awake();
    }

    protected override bool IsMyState(State state)
    {
        string name = state.Name;
        return base.IsMyState(state)
            && name.Substring(0, 4) == "Menu";
    }
}
