using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Highest level state manager, controls the game flow 
public class StateManager : StateManagerBase
{
    public static StateManager Instance;

    private IEnumerator StateFlow()
    {
        while(true)
        {
            yield return SetState(MenuManager.Instance.Main());
            yield return SetState(GameManager.Instance.Reset());
            //yield return new WaitForSeconds(100);
            yield return SetState(GameManager.Instance.Game());
            yield return SetState(MenuManager.Instance.Score());
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
                states.Add(s.Name, s);
            }
        }
        StartCoroutine(StateFlow());
    }
}
