using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Base class which all state managers inherit from
//All states trigger begin and end events which can be added to using AddEventMethod

public abstract class StateManagerBase : MonoBehaviour
{
    protected Dictionary<System.Type, State> states;
    protected Coroutine currentRoutine;
    protected static State[] allStates;
    protected static Dictionary<string, State> cameraTransitions;

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        states = new Dictionary<System.Type, State>();

        if (allStates == null)
        {
            allStates = FindObjectsOfType<State>();
            cameraTransitions = new Dictionary<string, State>();
            foreach(var state in allStates)
            {
                if(state is CameraTransition)
                {
                    cameraTransitions.Add(state.Name.ToLower().Trim(), state);
                }
            }
        }

        foreach(var state in allStates)
        {
            if (IsMyState(state))
                states.Add(state.GetType(), state);
        }
    }

    protected virtual bool IsMyState(State state)
    {
        return !cameraTransitions.ContainsKey(state.Name.ToLower().Trim());
    }

    protected Coroutine CameraTransition(string transitionName)
    {
        transitionName = transitionName.ToLower().Trim();
        #if UNITY_EDITOR
        if (cameraTransitions.TryGetValue(transitionName,out State state))
        #endif
        {
            currentRoutine = StartCoroutine(state.Routine());
            return currentRoutine;
        }
        #if UNITY_EDITOR
        else
        {
            Debug.LogError("No camera transition with name \"" + transitionName + "\" found");
            return null;
        }
        #endif

    }

    protected virtual Coroutine SetState(System.Type stateType)
    {
        #if UNITY_EDITOR
        if (states.ContainsKey(stateType))
        #endif
        {
            currentRoutine = StartCoroutine(states[stateType].Routine());
            return currentRoutine;
        }
        #if UNITY_EDITOR
        else
        {
            Debug.LogError(GetType() + " does not contain state \"" + stateType.Name + "\"");
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
    public void AddEventMethod(System.Type stateType, string eventName, System.Action method)
    {
        if (!states.ContainsKey(stateType))
        {
            Debug.LogError(GetType() + " does not contain state \"" + stateType.Name + "\"");
            return;
        }
        states[stateType].AddToEvent(eventName, method);
    }
}
