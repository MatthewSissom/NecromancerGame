using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : StateManagerBase
{
    public static GameManager Instance;

    protected override void Awake()
    {
        base.Awake();
        if (Instance)
            Destroy(this);
        else
            Instance = this;
    }

    public IEnumerator Game()
    {
        InputManager.Instance.enabled = true;
        Conveyor conveyor = GameObject.FindObjectOfType<Conveyor>();
        conveyor.CreateBones();

        //yield return SetState("CountDown");

        //bone delivery loop
        while (conveyor.running)
        {
            yield return SetState("GhostTrans");
            yield return SetState("Conveyor");
            yield return new WaitForSeconds(3.0f);
            yield return SetState("TableTrans");
            yield return new WaitForSeconds(5.0f);
        }
        yield return SetState("CalculateScore");

        InputManager.Instance.clear();
        InputManager.Instance.enabled = true;
        yield break;
    }

    override protected void Start()
    {
        base.Start();
        foreach (State s in allStates)
        {
            string name = s.Name;
            string managerKey = name.Substring(0, 4);
            if (managerKey != "StMn" && managerKey != "Menu")
            {
                states.Add(s.Name, s);
            }
        }
    }
}
