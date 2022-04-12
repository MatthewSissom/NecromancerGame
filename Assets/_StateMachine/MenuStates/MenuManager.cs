using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : StateManagerBase
{
    public static MenuManager Instance;
    private string state;

    public IEnumerator InMenus(string menuState = "Main")
    {
        GoToMenu(menuState);
        CameraTransition("MenuBoardFlipped");
        yield return CameraTransition("MenuTransMain");

        // Show splash screen if entering the main menu
        if(menuState == "Main")
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
                    yield break;
                case "Options":
                    state = null;
                    yield return CameraTransition("MenuBoardFlipped");
                    yield return SetState(typeof(MenuOptions));
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
