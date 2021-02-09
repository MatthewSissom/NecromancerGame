using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : StateManagerBase
{
    public static MenuManager Instance;
    public string state;

    public IEnumerator InMenus()
    {
        while (true)
        {
            switch (state)
            {
                case "Main":
                    state = null;
                    yield return SetState("TransMain");
                    yield return SetState("BoardMain");
                    yield return SetState("Main");
                    break;
                case "Play":
                    yield break;
                case "Instructions":
                    state = null;
                    yield return SetState("BoardFlipped");
                    yield return SetState("Instructions");
                    break;
                case "Score":
                    state = null;
                    yield return SetState("TransMain");
                    yield return SetState("BoardFlipped");
                    yield return SetState("DisplayScore");
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
        base.Awake();
        if (Instance)
            Destroy(gameObject);
        else
            Instance = this;

        foreach (State s in allStates)
        {
            string name = s.Name;
            if (name.Substring(0, 4) == "Menu")
            {
                states.Add(s.Name.ToLower().Trim(), s);
            }
        }
    }

    protected override Coroutine SetState(string stateName)
    {
        return base.SetState("Menu" + stateName);
    }

}
