using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Cat behavior is in charge of directing midlevel goals like pathing, pawing at something, looking at something etc.
//Recives instructions from cat goals which directs high level goals 

public struct PathTunables
{
    public float minTurningRad;
    public float speed;

    public PathTunables(float minTurningRad, float speed)
    {
        this.minTurningRad = minTurningRad;
        this.speed = speed;
    }
}

public class SkeletonBehaviour : MonoBehaviour
{

    [Header("LimbEnds")]
    [SerializeField]
    public GameObject followTarget;
    Vector3 targetPreviousPos = new Vector3(-1000,-1000,-1000);
    bool pathing = false;

    [SerializeField]
    float speed;

    [SerializeField]
    List<LimbData> limbEnds;
    [SerializeField]
    float stepHeight;
    [SerializeField]
    float chestHeight;
    [SerializeField]
    float hipDelay;

    [SerializeField]
    private Transform hipTransform;
    [SerializeField]
    private Transform headTransform;
    [SerializeField]
    private Transform tailTransform;

    public bool Initalized { get; private set; } = false;
    
    //holds transforms along the main line of the cat 
    Transform[] orderedTransforms;

    SkeletonMovement movement;
    SkeletonPathfinding pathfinding;
    SkeletonBasePathBuilder pathBuilder;

    // if cat can't change paths immeditely (jumping) queue it instead
    public Vector3? queuedDestination = null;

    //temp
    float timer;

    public void BehaviorInit(List<LimbData> limbEnds, Transform[] orderedTransforms, float[] transformDistances, int shoulderIndex)
    {
        if (Initalized)
            return;
        Initalized = true;

        UpdateTunables(new PathTunables(
            .1f,
            speed
        ));

        this.orderedTransforms = orderedTransforms;
        this.limbEnds = limbEnds;

        speed = 0.3f;
        stepHeight = 0.05f;

        for (int i = 0; i < transformDistances.Length; i++)
        {
            transformDistances[i] /= speed;
            if (i > shoulderIndex)
                transformDistances[i] *= -1;
        }

    }



    private void Start()
    {
        void CleanUp()
        {
            GameManager.Instance.RemoveEventMethod(typeof(GameCleanUp), "Begin", CleanUp);
            Destroy(gameObject.transform.root.gameObject);
        }
        if(GameManager.Instance)
            GameManager.Instance.AddEventMethod(typeof(GameCleanUp), "Begin", CleanUp);

        if (Initalized)
            return;
        Initalized = true;

        Debug.Log("Start init used on catbehavior. This should only happen when ");

        orderedTransforms = new Transform[4];
        orderedTransforms[0] = tailTransform;
        orderedTransforms[1] = hipTransform;
        orderedTransforms[2] = transform;
        orderedTransforms[3] = headTransform;

        float[] delays= new float[4];
        delays[0] = (tailTransform.position - hipTransform.position).magnitude / speed * 2 + (transform.position - hipTransform.position).magnitude / speed + hipDelay;
        delays[1] = (transform.position - hipTransform.position).magnitude / speed + hipDelay;
        delays[2] = 0;
        delays[3] = -(transform.position - headTransform.position).magnitude / speed * 2;
    }

    bool PathToPoint(Vector3 destination)
    {
        targetPreviousPos = followTarget.transform.position;

        List<Vector3> destinations =  pathfinding.GetPathPoints(transform.position, destination);
        if (destinations == null)
            return false;

        IContinuousSkeletonPath basePath = pathBuilder.PathFromPoints(transform.position, transform.forward, destinations);
        if (basePath == null)
            return false;

        movement.SetPath(basePath);
        return true;
    }

    private void Update()
    {
        if(followTarget && (followTarget.transform.position - targetPreviousPos).magnitude > 0.05f)
            PathToPoint(followTarget.transform.position);
    }

    private void UpdateTunables(PathTunables tunables)
    {
        pathBuilder = new SkeletonBasePathBuilder(tunables);
        pathfinding = new SkeletonPathfinding(tunables);
    }
}