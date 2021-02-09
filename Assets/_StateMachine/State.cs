using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    public string Name { get; protected set; }
    public abstract IEnumerator Routine();

    //events for beginning and ending a state, Begin and End must be called 
    //at the begining and end of any implementation of Routine
    public delegate void StateMethod();
    protected event StateMethod BeginEvent;
    protected event StateMethod EndEvent;

    //tries to add a method to an event, returns true if the state was found, false if it was not
    //to add new states to a child of this class use if(!base.AddToEvent(eventName,method,true)) and 
    //add a new switch statement with all new events
    public virtual bool AddToEvent(string eventName, StateMethod method, bool overriden = false)
    {
        eventName = eventName.ToLower().Trim();
        switch(eventName)
        {
            case "begin":
                BeginEvent += method;
                return true;
            case "end":
                EndEvent += method;
                return true;
            default:
                if(!overriden) Debug.LogError(GetType() + " does not contain event \"" + eventName + "\"");
                return false;
        }
    }

    public virtual void SetName()
    {
        Name = this.GetType().ToString();
    }

    protected void Begin()
    {
        BeginEvent?.Invoke();
    }

    protected void End()
    {
        EndEvent?.Invoke();
    }

    protected virtual void Awake()
    {
        SetName();
    }

}
