using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Cat behavior is in charge of directing midlevel goals like pathing, pawing at something, looking at something etc.
//Recives instructions from cat goals which directs high level goals 

public struct PathTunables
{
    public float MinTurningRad { get; private set; }
    public float Speed { get; private set; }
    public float SpineLength { get; private set; }
    // add a small amount of extra time for buffer
    public float SkeletonDuration { get => SpineLength / Speed + .25f; }
    public float DelayedPathLenght { get => SkeletonDuration * Speed; }

    public PathTunables(float minTurningRad, float speed, float spineLenght)
    {
        MinTurningRad = minTurningRad;
        Speed = speed;
        SpineLength = spineLenght;
    }
}

public struct SkeletonLayoutData
{
    public LimbData[] LimbEnds;
    public SpinePointData[] SpinePoints;
    public float SkeletonLenght;

    public SkeletonLayoutData(LimbData[] limbEnds, SpinePointData[] spinePoints, float skeletonLength)
    {
        LimbEnds = limbEnds;
        SpinePoints = spinePoints;
        SkeletonLenght = skeletonLength;
    }
}

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
        do
        {
            Add(enumerator.Current);
        } while (enumerator.MoveNext());
    }
}

public class SkeletonBehaviour : MonoBehaviour
{

    [Header("LimbEnds")]
    [SerializeField]
    public GameObject followTarget;
    Vector3 targetPreviousPos = new Vector3(-1000,-1000,-1000);

    [SerializeField]
    float speed;

    // used to set up test cats, unused in game pipeline
    [SerializeField]
    List<LimbData> limbEnds;
    [SerializeField]
    float stepHeight;
    [SerializeField]
    float chestHeight; 

    [SerializeField]
    private Transform hipTransform;
    [SerializeField]
    private Transform headTransform;
    [SerializeField]
    private Transform tailTransform;

    public bool Initalized { get; private set; } = false;
    
    SkeletonMovement movement;
    SkeletonPathfinding pathfinding;
    ActionQueue actionQueue;
    SkeletonLocomotionPlanner locomotionPlanner;
    SkeletonBasePathBuilder pathBuilder;

    //temp
    float timer;

    public void BehaviorInit(SkeletonLayoutData initData)
    {
        if (Initalized)
            return;
        Initalized = true;

        // inits pathfinder and path builder
        UpdateTunables(new PathTunables(
            .1f,
            speed,
            initData.SkeletonLenght
        ));

        movement = new SkeletonMovement(initData.LimbEnds, initData.SpinePoints);
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

        Transform[] orderedTransforms = new Transform[4];
        orderedTransforms[0] = headTransform;
        orderedTransforms[1] = transform;
        orderedTransforms[2] = hipTransform;
        orderedTransforms[3] = tailTransform;

        float[] distances = new float[4];
        distances[0] = 0;
        distances[1] = (orderedTransforms[0].transform.position - orderedTransforms[1].transform.position).magnitude;
        distances[2] = (orderedTransforms[1].transform.position - orderedTransforms[2].transform.position).magnitude;
        distances[3] = (orderedTransforms[2].transform.position - orderedTransforms[3].transform.position).magnitude;
        float totalDistance = distances[1] + distances[2] + distances[3];

        // set spine delays
        SpinePointData[] spinePoints = new SpinePointData[orderedTransforms.Length];
        float cumulativeDelay = 0;
        for (int i = 0; i < spinePoints.Length; i++)
        {
            cumulativeDelay += distances[i] / speed;
            spinePoints[i] = new SpinePointData(
                orderedTransforms[i],
                cumulativeDelay
            );
        }

        // set limb delays to match their corresponding spine point (shoulders for front legs, hips for back legs)
        foreach(LimbData limbEnd in limbEnds)
        {
            switch (limbEnd.LocationTag)
            {
                case LimbLocationTag.FrontLeft:
                    limbEnd.SetDelay(spinePoints[1].Delay);
                    break;
                case LimbLocationTag.FrontRight:
                    limbEnd.SetDelay(spinePoints[1].Delay);
                    break;
                case LimbLocationTag.BackLeft:
                    limbEnd.SetDelay(spinePoints[2].Delay);
                    break;
                case LimbLocationTag.BackRight:
                    limbEnd.SetDelay(spinePoints[2].Delay);
                    break;
                default:
                    break;
            }
        }

        BehaviorInit(new SkeletonLayoutData(limbEnds.ToArray(), spinePoints, totalDistance));
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

    private void UpdateTunables(PathTunables tunables)
    {
        pathBuilder = new SkeletonBasePathBuilder(tunables);
        pathfinding = new SkeletonPathfinding(tunables);
    }

    private void CleanUp()
    {
        GameManager.Instance.RemoveEventMethod(typeof(GameCleanUp), "Begin", CleanUp);
        Destroy(gameObject.transform.root.gameObject);
    }
}