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
        {
            Destroy(this);
        }
        else
            Instance = this;

        foreach (State s in allStates)
        {
            string name = s.Name;
            string managerKey = name.Substring(0, 4);
            if (managerKey != "StMn" && managerKey != "Menu")
            {
                states.Add(s.Name.ToLower().Trim(), s);
            }
        }
    }

    public IEnumerator Game()
    {
        yield return SetState("GameInit");

        //bone delivery loop
        while (!GhostManager.Instance.done)
        {
            //yield return SetState("GhostTrans");
            yield return SetState("GhostManager");
            yield return SetState("TableTrans");
            yield return new WaitForSeconds(GhostManager.Instance.timeBetweenShipments);
        }

        // Testing assignment checker
        gameObject.GetComponent<AssignmentChecker>().AssignmentInit();
        gameObject.GetComponent<AssignmentChecker>().DFSearch(GameObject.FindGameObjectWithTag("Root").transform);
        gameObject.GetComponent<AssignmentChecker>().AssignmentCheck();

        yield return SetState("BoneAssembler");
        yield return SetState("CatWalkStart");
        yield return SetState("TestCatState");
        yield return SetState("CatWalkEnd");

        yield return SetState("CalculateScore");

        //yield return SetState("GameCleanUp");

        yield break;
    }
}
