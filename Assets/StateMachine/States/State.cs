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
    public void AddBeginMethod(StateMethod method)
    {
        BeginEvent += method;
    }
    public void AddEndMethod(StateMethod method)
    {
        EndEvent += method;
    }
    protected void Begin()
    {
        BeginEvent?.Invoke();
    }
    protected void End()
    {
        EndEvent?.Invoke();
    }
}
