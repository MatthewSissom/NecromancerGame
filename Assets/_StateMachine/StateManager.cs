using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Highest level state manager, controls the game flow 
public class StateManager : StateManagerBase
{
    public static StateManager Instance;

    private IEnumerator StateFlow()
    {
        MenuManager.Instance.GoToMenu("Main");
        yield return SetState(MenuManager.Instance.Main());
        while (true)
        {
            yield return SetState(GameManager.Instance.Game());
            MenuManager.Instance.GoToMenu("Score");
            yield return SetState(MenuManager.Instance.Main());
        }
    }

    override protected void Awake()
    {
        base.Awake();
        if (Instance)
            Destroy(this);
        else
            Instance = this;
    }
    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        foreach(State s in allStates)
        {
            string name = s.Name;
            if (name.Substring(0, 4) == "StMn")
            {
                states.Add(s.Name.ToLower().Trim(), s);
            }
        }
        StartCoroutine(StateFlow());
    }
}
