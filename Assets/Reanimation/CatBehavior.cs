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
    [SerializeField]
    float speed;

    [SerializeField]
    List<LimbEnd> limbEnds;
    [SerializeField]
    float stepHeight;
    [SerializeField]
    float chestHeight;
    Dictionary<Collider, LimbEnd> colliderToLimb;


    [SerializeField]
    Vector3 shoulderVelocity;
    [SerializeField]
    Vector3 hipVelocity;

    CatMovement movement;
    CatPath mPath;

    float groundYVal;


    CatStablizer stablizer;
    bool stablizing = false;

    private void Awake()
    {
        mPath = new CatPath();
        movement = new CatMovement(limbEnds,gameObject, chestHeight, mPath);

        LimbInit();

        stablizer = new CatStablizer(null, 1000, groundYVal);
        stablizer.DestablizedEvent += () => { stablizing = false; Debug.Log("Fallen Cat!"); };

        foreach(var limb in limbEnds)
        {
            stablizer.DestablizedEvent += limb.Destableized;
        }
    }


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
            limb.TempLimbInit();

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
        
        if(stablizing)
            stablizer.Update(Time.deltaTime);
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
