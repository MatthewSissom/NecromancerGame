using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Base class which all state managers inherit from
//All states trigger begin and end events which can be added to using AddEventMethod

public abstract class StateManagerBase : MonoBehaviour
{
    protected Dictionary<System.Type, State> states;
    //current running coroutine/state
    protected Coroutine currentRoutine;
    //Aray of States
    protected static State[] allStates;
    protected static Dictionary<string, State> cameraTransitions;

    // Populating states and camera Transition dictionaries
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

    /// <summary>
    /// Checks to see if given state belongs to this state manager.
    /// i.e. Bone Delivery belongs to GameManager because it's part of the main game
    /// </summary>
    /// <param name="state">Object to check if it is a state</param>
    /// <returns></returns>
    protected virtual bool IsMyState(State state)
    {
        return !cameraTransitions.ContainsKey(state.Name.ToLower().Trim());
    }

    protected Coroutine CameraTransition(string transitionName)
    {
        transitionName = transitionName.ToLower().Trim();
        if (cameraTransitions.TryGetValue(transitionName,out State state))
        {
            currentRoutine = StartCoroutine(state.Routine());
            return currentRoutine;
        }
        else
        {
            Debug.LogError("No camera transition with name \"" + transitionName + "\" found");
            return null;
        }

    }

    /// <summary>
    /// Sets the Game State
    /// </summary>
    /// <param name="stateType">State of game to change to</param>
    /// <returns></returns>
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

    public void RemoveEventMethod(System.Type stateType, string eventName, System.Action method)
    {
        if (!states.ContainsKey(stateType))
        {
            Debug.LogError(GetType() + " does not contain state \"" + stateType.Name + "\"");
            return;
        }
        states[stateType].RemoveFromEvent(eventName, method);
    }
}
