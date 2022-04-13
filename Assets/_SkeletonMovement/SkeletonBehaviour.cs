using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Cat behavior is in charge of directing midlevel goals like pathing, pawing at something, looking at something etc.
//Recives instructions from cat goals which directs high level goals 

public class ActionQueue
{
    public bool CurrentQueueIsActive { get; private set; }
    public bool Empty { get => actionQueue.Count == 0; }

    private Queue<Action> actionQueue;

    public ActionQueue()
    {
        Clear();
    }

    public Action SetNextAsActive()
    {
        if (Empty)
            return null;
        CurrentQueueIsActive = true;
        return actionQueue.Dequeue();
    }

    public void Clear()
    {
        actionQueue = new Queue<Action>();
        CurrentQueueIsActive = false;
    }

    public void Add(Action action)
    {
        actionQueue.Enqueue(action);
    }

    public void Add(IEnumerable<Action> actions)
    {
        IEnumerator<Action> enumerator = actions.GetEnumerator();
        while (enumerator.MoveNext())
        {
            Add(enumerator.Current);
        }
    }
}

public class SkeletonBehaviour : MonoBehaviour
{

    [Header("LimbEnds")]
    [SerializeField]
    public GameObject followTarget;
    Vector3 targetPreviousPos = new Vector3(-1000,-1000,-1000);

    [field: SerializeField]
    public float Speed { get; private set; }

    // used to set up test cats, unused in game pipeline
    [field: SerializeField]
    public LimbTunables LimbTunables { get; private set; }
    [SerializeField]
    SkeletonTransforms skeletonPositions;
    [SerializeField]
    SkeletonTransforms targets;
    [SerializeField]
    List<LimbData> limbEnds;

    public bool Initalized { get; private set; } = false;
    
    SkeletonMovement movement;
    SkeletonPathfinding pathfinding;
    ActionQueue actionQueue;
    SkeletonLocomotionPlanner locomotionPlanner;
    SkeletonBasePathBuilder pathBuilder;

    //temp
    float timer;

    public void BehaviorInit(SkeletonLayoutData initData, SkeletonPathTunables tunables)
    {
        if (Initalized)
            return;
        Initalized = true;

        movement = new SkeletonMovement(initData.LimbEnds, initData.SpinePoints);

        // inits pathfinder and path builder
        UpdateTunables(
            tunables,
            initData
        );

        locomotionPlanner = new SkeletonLocomotionPlanner(pathBuilder, initData);
        actionQueue = new ActionQueue();

        //speed = 0.3f;
        //stepHeight = 0.05f;
    }



    private void Start()
    {
        if(GameManager.Instance)
            GameManager.Instance.AddEventMethod(typeof(GameCleanUp), "Begin", CleanUp);

        if (Initalized)
            return;

        Debug.Log("Start init used on catbehavior. This should only happen when using the playpen debug mode");

        var tunables = new SkeletonPathTunables(.1f, Speed ); 

        MovementDataInit dataInit = new MovementDataInit();
        dataInit.EditorInit(limbEnds.ToArray(), skeletonPositions, targets, tunables);
        SkeletonLayoutData layoutData = dataInit.ComputedLayoutData;
        BehaviorInit(layoutData,tunables);
    }

    private bool MoveToPoint(Vector3 destination)
    {
        targetPreviousPos = followTarget.transform.position;
        Vector3 currentPathPos = movement.GetPathPos();

        // get list of points to visit from unity's nav mesh
        List<Vector3> destinations =  pathfinding.GetPathPoints(currentPathPos, destination);
        if (destinations == null)
            return false;

        // plan actions to move between destinations
        Queue<Action> actions = locomotionPlanner.PlanMovementFromDestinations(destinations);
        if (actions == null)
            return false;
        actionQueue.Clear();
        actionQueue.Add(actions);

        return true;
    }

    private void Update()
    {
        if(followTarget && (followTarget.transform.position - targetPreviousPos).magnitude > 0.05f)
            MoveToPoint(followTarget.transform.position);

        // update movement and check if action needs to be changed
        movement.Update(Time.deltaTime);
        if (movement.CanSetAction && !actionQueue.Empty)
        {
            // always switch to current queue if it's not already active
            if (!actionQueue.CurrentQueueIsActive)
                movement.SetAction(actionQueue.SetNextAsActive());

            // if current action is finished, start the next action
            if (movement.ActionCompletion == 1)
                movement.SetAction(actionQueue.SetNextAsActive());
        }
    }

    private void UpdateTunables(SkeletonPathTunables tunables, SkeletonLayoutData layoutData)
    {
        pathfinding = new SkeletonPathfinding(tunables);
        pathBuilder = new SkeletonBasePathBuilder(tunables, new SkeletonPathData(tunables,layoutData), movement);
    }

    private void CleanUp()
    {
        GameManager.Instance.RemoveEventMethod(typeof(GameCleanUp), "Begin", CleanUp);
        Destroy(gameObject.transform.root.gameObject);
    }
}