using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : StateManagerBase
{
    public static GameManager Instance;
    private FMOD.Studio.EventInstance musicInstance;

    protected override void Awake()
    {
        base.Awake();

        if (Instance)
        {
            Destroy(this);
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        else
            Instance = this;
    }

    public IEnumerator Game()
    {
        yield return SetState("GameInit");
        
        //yield return SetState("CountDown");

        //bone delivery loop
        while (!GhostManager.Instance.done)
        {
            yield return SetState("GhostTrans");
            yield return SetState("GhostManager");
            yield return SetState("TableTrans");
            yield return new WaitForSeconds(5.0f);
        }
        yield return SetState("CalculateScore");

        yield return SetState("GameCleanUp");

        yield break;
    }

    public IEnumerator Reset()
    {
        //TODO implement reset
        yield break;
    }

    override protected void Start()
    {
        base.Start();

        musicInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Music/NecromancerAcademyDemo_Take4");
        musicInstance.start();

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
