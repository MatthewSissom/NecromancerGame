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
            currentRoutine = StartCoroutine(states[stateName].Routine());
            return currentRoutine;
        }
        #if UNITY_EDITOR
        else
        {
            Debug.LogError("State Manager does not contain state \"" + stateName + "\"");
            return null;
        }
        #endif
    }

    protected virtual Coroutine SetState(IEnumerator toStart)
    {
        currentRoutine = StartCoroutine(toStart);
        return currentRoutine;
    }

    public void AddStateBeginMethod(string stateName, State.StateMethod method)
    {
        IEnumerator HookUpEvent()
        {
            yield return new WaitForSeconds(0.1f);
            if (!states.ContainsKey(stateName))
            {
                Debug.LogError("State Manager does not contain state \"" + stateName + "\"");
                yield break;
            }
            states[stateName].AddBeginMethod(method);
            yield break;
        }
        StartCoroutine(HookUpEvent());
    }

    public void AddStateEndMethod(string stateName, State.StateMethod method)
    {
        IEnumerator HookUpEvent()
        {
            yield return new WaitForSeconds(0.1f);
            if (!states.ContainsKey(stateName))
            {
                Debug.LogError("State Manager does not contain state \"" + stateName + "\"");
                yield break;
            }
            states[stateName].AddEndMethod(method);
            yield break;
        }
        StartCoroutine(HookUpEvent());
    }
}
