using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Cat behavior is in charge of directing midlevel goals like pathing, pawing at something, looking at something etc.
//Recives instructions from cat goals which directs high level goals 
public class CatBehavior : MonoBehaviour
{

    [Header("LimbEnds")]
    [SerializeField]
    public GameObject followTarget;
    Vector3 targetPreviousPos = new Vector3(-1000,-1000,-1000);
    bool pathing = false;

    [SerializeField]
    float speed;

    [SerializeField]
    List<LimbEnd> limbEnds;
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

    CatMovement movement;
    CatPath mPath;

    //temp
    float timer;

    public float GroundHeight
    {
        get { return movement.GroundYValue; }
        set { movement.SetGroundYValue(value);
            (mPath as CatPathWithNav).GroundHeight = value;
        }
    }

    public float ChestHeight
    {
        get { return movement.ChestHeight; }
    }

    public void BehaviorInit(List<LimbEnd> limbEnds, Transform[] orderedTransforms, float[] transformDistances, int shoulderIndex)
    {
        if (Initalized)
            return;
        Initalized = true;

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

        movement = new CatMovement(limbEnds,speed);
        var catPathWithNav = new CatPathWithNav(transform.position.y-movement.GroundYValue, transformDistances, orderedTransforms,shoulderIndex);
        catPathWithNav.GroundHeight = movement.GroundYValue;
        mPath = catPathWithNav;
        mPath.PathFinished += () => { pathing = false; };
        mPath.PathStarted += () => { pathing = true; };
        movement.SetPath(catPathWithNav, limbEnds);
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

        Debug.LogError("Start init used on catbehavior");

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

        movement = new CatMovement(limbEnds,speed);
        var temp = new CatPathWithNav(ChestHeight, delays, orderedTransforms,2);
        temp.GroundHeight = movement.GroundYValue;
        mPath = temp;
        mPath.PathFinished += () => { pathing = false; };
        mPath.PathStarted += () => { pathing = true; };
        movement.SetPath(temp, limbEnds);

        //stablizer = new CatStablizer(null, 1000, groundYVal);
        //stablizer.DestablizedEvent += () => { stablizing = false; Debug.Log("Fallen Cat!"); };
        //foreach(var limb in limbEnds)
        //{
        //    stablizer.DestablizedEvent += limb.Destableized;
        //}
    }

    void PathToPoint(Vector3 destination)
    {
        //destination.y = movement.ChestHeight;
        mPath.PathToPoint(destination);
        targetPreviousPos = followTarget.transform.position;
    }

    private void Update()
    {
        if(followTarget && (followTarget.transform.position - targetPreviousPos).magnitude > 0.05f)
            PathToPoint(followTarget.transform.position);

        Vector3[] vectors = new Vector3[4];
        if (pathing)
        {
            //get base positions from path
            mPath.Move(Time.deltaTime, out Vector3 forward, vectors);

            ////modify positions based on offsets
            foreach (var limb in limbEnds)
            {
                vectors[limb.DelayIndex] += new Vector3(0, limb.HeightOffset, 0);
            }
            timer += Time.deltaTime;
            vectors[0] += new Vector3(0, 0.07f + Mathf.Sin(timer * 1.5f * Mathf.PI) * stepHeight / 4, 0);


            //apply new positions
            transform.forward = forward;
            for(int i = 0, count = orderedTransforms.Length; i < count;  i++)
            {
                orderedTransforms[i].position = vectors[i];
            }
        }

    }


    //private void OnCollisionEnter(Collision collision)
    //{
    //    //pass on collisions to limbs
    //    for (int i = 0; i < collision.contactCount; i++)
    //    {
    //        var contact = collision.GetContact(i);
    //        if(colliderToLimb.TryGetValue(contact.thisCollider,out LimbEnd collidedLimb))
    //            collidedLimb.Collided(contact.point);
    //    }
    //}
}