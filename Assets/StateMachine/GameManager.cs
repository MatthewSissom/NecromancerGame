using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private Dictionary<string, State> states;
    private Coroutine currentRoutine;
    public string currentState { get; private set; }

    private IEnumerator StateFlow()
    {
        while(true)
        {
            yield return SetState("MainMenu");
            yield return SetState("CameraTransition1");
            yield return SetState("CountDown");
            yield return SetState("Conveyor");
        }
    }

    public void AddStateBeginMethod(string stateName, State.StateMethod method)
    {
        if (!states.ContainsKey(stateName))
        {
            Debug.LogError("State Manager does not contain state \"" + stateName + "\"");
            return;
        }
        states[stateName].AddBeginMethod(method);
    }

    public void AddStateEndMethod(string stateName, State.StateMethod method)
    {
        if (!states.ContainsKey(stateName))
        {
            Debug.LogError("State Manager does not contain state \"" + stateName + "\"");
            return;
        }
        states[stateName].AddEndMethod(method);
    }

    private Coroutine SetState(string stateName)
    {
        if (states.ContainsKey(stateName))
        {
            currentRoutine = StartCoroutine(states[stateName].Routine());
            return currentRoutine;
        }
        else
        {
            Debug.LogError("State Manager does not contain state \"" + stateName + "\"");
            return null;
        }
    }

    private void Awake()
    {
        if (Instance)
            Destroy(gameObject);
        else
            Instance = this;

        states = new Dictionary<string, State>();

    }
    // Start is called before the first frame update
    void Start()
    {
        var allStates = GameObject.FindObjectsOfType<State>();
        foreach(State s in allStates)
        {
            states.Add(s.Name, s);
        }
        StartCoroutine(StateFlow());
    }
}
