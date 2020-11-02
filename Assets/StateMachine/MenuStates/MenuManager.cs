using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : StateManagerBase
{
    public static MenuManager Instance;
    public string buttonName;

    public IEnumerator Main()
    {
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            //camera + board transition
            yield return SetState("TransMain");
            yield return SetState("BoardMain");
            yield return SetState("Main");
            switch (buttonName)
            {
                case "Play":
                    buttonName = null;
                    yield break;
                case "Instructions":
                    buttonName = null;
                    yield return SetState("BoardFlipped");
                    yield return SetState("Instructions");
                    break;
                default:
                    buttonName = null;
                    yield break;
            }
        }
    }

    public IEnumerator Score()
    {
        yield return SetState("TransMain");
        yield return SetState("BoardFlipped");
        yield return SetState("DisplayScore");
    }

    protected override void Awake()
    {
        base.Awake();
        if (Instance)
            Destroy(gameObject);
        else
            Instance = this;
    }

    override protected void Start()
    {
        base.Start();
        foreach (State s in allStates)
        {
            string name = s.Name;
            if (name.Substring(0, 4) == "Menu")
            {
                states.Add(s.Name, s);
            }
        }
    }

    protected override Coroutine SetState(string stateName)
    {
        return base.SetState("Menu" + stateName);
    }

}
