using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void AddEventMethod(string stateName, string eventName, State.StateMethod method)
    {
        IEnumerator HookUpEvent()
        {
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
