using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : StateManagerBase
{
    public static StateManager Instance;

    private IEnumerator StateFlow()
    {
        yield return new WaitForEndOfFrame();
        while(true)
        {
            yield return SetState(MenuManager.Instance.Routine());
            yield return SetState("GameTrans");
            yield return SetState("CountDown");
            yield return SetState("Conveyor");
        }
    }

    override protected void Awake()
    {
        base.Awake();
        if (Instance)
            Destroy(gameObject);
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
            if (name.Substring(0, 4) != "Menu")
            {
                states.Add(s.Name, s);
            }
        }
        StartCoroutine(StateFlow());
    }
}
