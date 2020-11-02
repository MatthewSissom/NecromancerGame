﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Base class which all state managers inherit from
//All states trigger begin and end events which can be added to using AddEventMethod

public class StateManagerBase : MonoBehaviour
{
    protected Dictionary<string, State> states;
    protected Coroutine currentRoutine;
    protected static State[] allStates;

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        states = new Dictionary<string, State>();
    }

    protected virtual void Start()
    {
        if (allStates == null)
        {
            allStates = GameObject.FindObjectsOfType<State>();
        }
    }

    protected virtual Coroutine SetState(string stateName)
    {
        stateName = stateName.ToLower().Trim();
        #if UNITY_EDITOR
        if (states.ContainsKey(stateName))
        #endif
        {
            //Debug.Log(GetType() + "setting state to" + stateName);
            currentRoutine = StartCoroutine(states[stateName].Routine());
            return currentRoutine;
        }
        #if UNITY_EDITOR
        else
        {
            Debug.LogError(GetType() + " does not contain state \"" + stateName + "\"");
            return null;
        }
        #endif
    }

    protected virtual Coroutine SetState(IEnumerator toStart)
    {
        currentRoutine = StartCoroutine(toStart);
        return currentRoutine;
    }

    //Used to run code when a state event is triggered
    //All states have a "begin" and "end" event and can implement custom events as well.
    public void AddEventMethod(string stateName, string eventName, State.StateMethod method)
    {
        IEnumerator HookUpEvent()
        {
            stateName = stateName.ToLower().Trim();
            yield return new WaitForSeconds(0.2f);
            if (!states.ContainsKey(stateName))
            {
                Debug.LogError(GetType() + " does not contain state \"" + stateName + "\"");
                yield break;
            }
            states[stateName].AddToEvent(eventName, method);
            yield break;
        }
        StartCoroutine(HookUpEvent());
    }
}
