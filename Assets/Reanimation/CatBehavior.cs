using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Cat behavior is in charge of directing midlevel goals like walking / pathing, pawing at something, looking at something etc.
//Recives instructions from cat goals which directs high level goals 
public class CatBehavior : MonoBehaviour
{

    [Header("LimbEnds")]
    [SerializeField]
    GameObject followTarget;
    [SerializeField]
    float speed;
    [SerializeField]
    float footPeriod;

    [SerializeField]
    List<LimbEnd> limbEnds;
    [SerializeField]
    float stepHeight;
    [SerializeField]
    float chestHeight;
    Dictionary<Collider, LimbEnd> colliderToLimb;

    //TEMP
    [SerializeField]
    Vector3 toMove = new Vector3();

    CatMovement movement;
    float groundYVal;


    CatStablizer stablizer;
    bool destablized = false;

    private void Awake()
    {
        movement = new CatMovement(limbEnds,gameObject);
        movement.CommandsExaustedEvent += AddCommands;

        LimbInit();

        stablizer = new CatStablizer(GetComponent<Rigidbody>(), 1000, groundYVal);
        stablizer.DestablizedEvent += () => { destablized = true; Debug.Log("Fallen Cat!"); };
        foreach(var limb in limbEnds)
        {
            stablizer.DestablizedEvent += limb.Destableized;
        }


        AddCommands();
        
        IEnumerator WaitForStep()
        {
            yield return new WaitForSeconds(0.1f);
            movement.StepWithNextLimb();
        }
        StartCoroutine(WaitForStep());
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
            limb.TempLimbInit(chestHeight);

            //TEMP movement
            limb.StepEndEvent += (LimbEnd calling, Vector3? collision) => { toMove += new Vector3(0, 0, speed * footPeriod / 4); };

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

    private void AddCommands(CatMovement.LimbMovementCommand current = default)
    {
        Vector3 toMove = new Vector3(0, 0, speed * footPeriod/ 4);
        for(int i = 0; i < limbEnds.Count; i++)
        {
            Vector3 newTarget = limbEnds[i].LimbStart.transform.position;
            newTarget.y = groundYVal;
            newTarget += toMove * (i + 1);

            movement.AddCommand(limbEnds[i],newTarget);
        }
    }

    private void Update()
    {
        float toMoveDist = toMove.magnitude;
        if (toMoveDist > 0)
        {
            float distanceMoved = speed  *Time.deltaTime;
            Vector3 distVec = new Vector3(0, 0, distanceMoved);
            transform.position += distVec;
            toMove -= distVec;
            if (distanceMoved > toMoveDist)
                toMove = new Vector3();
        }
        //if(!destablized)
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
