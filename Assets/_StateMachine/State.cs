using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    //the name of this state
    public string Name 
    { 
        get 
        {
            if(stateName == null)
                SetName();
            return stateName;
        }
        protected set { stateName = value; }
    }
    private string stateName;

    //the routine that runs during this state
    public abstract IEnumerator Routine();

    //events for beginning and ending a state, Begin and End must be called 
    //at the begining and end of any implementation of Routine
    protected class EventWraper
    { 
        protected event System.Action mEvent;
        public void Subscribe(System.Action listener)
        {
            mEvent += listener;
        }
        public void Remove(System.Action listener)
        {
            mEvent -= listener;
        }
        public void Invoke()
        {
            mEvent?.Invoke();
        }
    }

    protected Dictionary<string, EventWraper> allEvents;

    //tries to add a method to an event, returns true if the state was found, false if it was not
    //to add new states to a child of this class use if(!base.AddToEvent(eventName,method,true)) and 
    //add a new switch statement with all new events
    public bool AddToEvent(string eventName, System.Action method)
    {
        eventName = eventName.ToLower().Trim();
        if (allEvents.TryGetValue(eventName, out var mEvent))
        {
            mEvent.Subscribe(method);
            return true;
        }
        else
        {
            Debug.LogError(GetType() + " does not contain event \"" + eventName + "\"");
            return false;
        }
    }

    public bool RemoveFromEvent(string eventName, System.Action method)
    {
        eventName = eventName.ToLower().Trim();
        if (allEvents.TryGetValue(eventName, out var mEvent))
        {
            mEvent.Remove(method);
            return true;
        }
        else
        {
            Debug.LogError(GetType() + " does not contain event \"" + eventName + "\"");
            return false;
        }
    }

    public virtual void SetName()
    {
        Name = this.GetType().ToString();
    }

    protected void Begin()
    {
        allEvents["begin"]?.Invoke();
    }

    protected void End()
    {
        allEvents["end"]?.Invoke();
    }

    protected virtual void Awake()
    {
        SetName();
        allEvents = new Dictionary<string, EventWraper>();
        allEvents.Add("begin", new EventWraper());
        allEvents.Add("end", new EventWraper());
    }

}
