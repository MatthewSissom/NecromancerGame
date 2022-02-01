using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Class to store what actions a ghost might take AFTER a given point 


public class PathData : MonoBehaviour
{
    public enum GhostAction
    {
        None,
        Rush,
        Meander,
        Forget,
        Skip,
    }
    [SerializeField]
    GhostAction action = GhostAction.None;

    public GhostAction Action
    {
        get { return action; }
    }

    /*/// <summary>
    /// debug method for knowing actions
    /// </summary>
    public string ActionToString()
    {
        switch (action)
        {
            case GhostAction.None:
                return "Action = None";
            case GhostAction.Forget:
                return "Action = Forget";
            case GhostAction.Meander:
                return "Action = Meander";
            case GhostAction.Rush:
                return "Action = Rush";
            case GhostAction.Skip:
                return "Action = Skip";
            default:
                return "Action doesn't equal anything this shouldn't be possible the world is ending!!!!!!!";
        }
    }*/



}
