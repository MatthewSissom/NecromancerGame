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
    Dictionary<Collider, LimbEnd> colliderToLimb;

    [SerializeField]
    private Transform hipTransform;

    CatMovement movement;
    CatPath mPath;

    float groundYVal;


    CatStablizer stablizer;
    bool stablizing = false;

    private void Awake()
    {
        //mPath = new CatPath();
        LimbInit();
        mPath = new CatPathWithNav(groundYVal,transform.position.y);
        mPath.HipDelay = (transform.position - hipTransform.position).magnitude / speed + hipDelay;
        movement = new CatMovement(limbEnds,gameObject, groundYVal, speed,mPath);


        stablizer = new CatStablizer(null, 1000, groundYVal);
        stablizer.DestablizedEvent += () => { stablizing = false; Debug.Log("Fallen Cat!"); };

        foreach(var limb in limbEnds)
        {
            stablizer.DestablizedEvent += limb.Destableized;
        }
    }

    //temp move to movement
    private void LimbInit()
    {
        groundYVal = 0;
        foreach (var limb in limbEnds)
        {
            groundYVal += limb.transform.position.y;
        }
        groundYVal /= limbEnds.Count;

        colliderToLimb = new Dictionary<Collider, LimbEnd>();
        foreach (var limb in limbEnds)
        {
            limb.StepSpeed = speed * 4;
            limb.StepHeight = stepHeight;
            limb.TempLimbInit(groundYVal);

            void RecursiveColliderSearch(Transform toCheck)
            {
                if (toCheck.TryGetComponent(out Collider c))
                    colliderToLimb.Add(c, limb);

                for (int i = 0; i < toCheck.childCount; i++)
                    RecursiveColliderSearch(toCheck.GetChild(i));
            }
            RecursiveColliderSearch(limb.transform);
        }
    }

    private void Update()
    {
        if(followTarget.transform.position != targetPreviousPos)
        {
            Vector3 temp = followTarget.transform.position;
            temp.y = transform.position.y;
            mPath.PathToPoint(temp,transform.position, hipTransform.position, transform.forward);
            targetPreviousPos = followTarget.transform.position;
            pathing = true;
        }
        if(stablizing)
            stablizer.Update(Time.deltaTime);
        if (pathing && (pathing = mPath.Move(Time.deltaTime, out Vector3 shoulderPos, out Vector3 hipPos, out Vector3 forward)))
        {
            transform.forward = forward;
            transform.position = shoulderPos;
            hipTransform.position = hipPos;  //move hips after rotation so they aren't rotated
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //pass on collisions to limbs
        for (int i = 0; i < collision.contactCount; i++)
        {
            var contact = collision.GetContact(i);
            if(colliderToLimb.TryGetValue(contact.thisCollider,out LimbEnd collidedLimb))
                collidedLimb.Collided(contact.point);
        }
    }
}