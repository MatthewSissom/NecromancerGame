using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Cat behavior is in charge of directing midlevel goals like pathing, pawing at something, looking at something etc.
//Recives instructions from cat goals which directs high level goals 
public class CatBehavior : MonoBehaviour
{

    [Header("LimbEnds")]
    [SerializeField]
    GameObject followTarget;
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

    //holds transforms along the main line of the cat 
    Transform[] orderedTransforms;

    CatMovement movement;
    CatPath mPath;



    CatStablizer stablizer;
    bool stablizing = false;

    private void Awake()
    {
        //mPath = new CatPath();
        orderedTransforms = new Transform[4];
        orderedTransforms[0] = tailTransform;
        orderedTransforms[1] = hipTransform;
        orderedTransforms[2] = transform;
        orderedTransforms[3] = headTransform;
        float[] delays= new float[4];
        delays[0] = (tailTransform.position - hipTransform.position).magnitude / speed;
        delays[1] = (transform.position - hipTransform.position).magnitude / speed + hipDelay;
        delays[2] = 0;
        delays[3] = -(transform.position - headTransform.position).magnitude / speed;
        mPath = new CatPathWithNav(transform.position.y, delays, orderedTransforms);
        mPath.PathFinished += () => { pathing = false; };

        movement = new CatMovement(limbEnds,stepHeight, speed,mPath);

        //stablizer = new CatStablizer(null, 1000, groundYVal);
        //stablizer.DestablizedEvent += () => { stablizing = false; Debug.Log("Fallen Cat!"); };
        //foreach(var limb in limbEnds)
        //{
        //    stablizer.DestablizedEvent += limb.Destableized;
        //}
    }

    void PathToPoint(Vector3 destination)
    {
        destination.y = transform.position.y;
        mPath.PathToPoint(destination);
        targetPreviousPos = followTarget.transform.position;
        pathing = true;
    }

    private void Update()
    {
        if(followTarget.transform.position != targetPreviousPos)
            PathToPoint(followTarget.transform.position);
        if(stablizing)
            stablizer.Update(Time.deltaTime);

        Vector3[] vectors = new Vector3[4];
        if (pathing)
        {
            mPath.Move(Time.deltaTime, out Vector3 forward, vectors);
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