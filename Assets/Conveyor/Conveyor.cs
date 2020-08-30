using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : State
{
    [Header("Bones")]
    public List<GameObject> orderedBones;
    public List<int> count;
    public boneManager boneManager;
    public List<int> bursts;

    [Header("Belt Stats")]
    public Vector3 velocity;
    public float boneSpacing;
    public Vector3 safeInstantiation;

    private Vector3 initalPos;
    private conveyorGroup mGroup;
    private Rigidbody mBody;
    private BoxCollider dropCollider;

    void CreateBones()
    {
        initalPos = mBody.position;

        //start all bones off screen and behind the conveyor
        Vector3 offset = transform.position + 
            -gameObject.GetComponentInChildren<Renderer>().bounds.extents.x * velocity.normalized
            + .5f * new Vector3(0,1,0);
        Vector3 offsetDirection = -velocity.normalized;

        //instantiate bones
        GameObject current;
        foreach(GameObject p in orderedBones)
        {
            current = Instantiate(p, safeInstantiation, p.transform.rotation);
            bone currentBone = current.GetComponent<bone>();
            if (currentBone)
            {
                currentBone.boneManager = boneManager;
                mGroup.addChild(currentBone.Group);

                //set bone position
                float length = getLength(current);
                offset += offsetDirection * length / 2;
                current.transform.position = offset;
                offset += offsetDirection * (length / 2 + boneSpacing);

                //don't use gravity until enabled
                currentBone.Rb.useGravity = false;
            }
            else
            {
                Destroy(current);
            }
            mGroup.GroupID = 0;
        }
    }

    private float getLength(GameObject bone)
    {
        return bone.GetComponent<Renderer>().bounds.size.x;
    }

    public void OnTriggerEnter(Collider collision)
    {
        bone b = collision.gameObject.GetComponentInParent<bone>();
        if (b)
        {
            b.Rb.useGravity = true;
            b.Rb.velocity = velocity + b.Rb.velocity.y * new Vector3(0,1,0);
            b.gameObject.SetActive(true);
            b.Group.removeFromConvayer();
        }
    }

    private IEnumerator BoneShipment(int count, float timeUntilNext)
    {
        float previousTime = Time.time;
        float elapsedTime = 0;
        int initalCount = mGroup.groupCount();
        while (mGroup.groupCount() > initalCount - count)
        {
            elapsedTime = Time.time - previousTime;
            previousTime = Time.time;
            mGroup.applyToAll((bone toApply, FunctionArgs e) =>
            {
                toApply.transform.Translate(velocity * elapsedTime, Space.World);
            }, new FunctionArgs());
            yield return null;
        }
        yield return new WaitForSeconds(timeUntilNext);
    }

    public override IEnumerator Routine()
    {
        CreateBones();
        Begin();
        yield return null;

        foreach(int burst in bursts)
        {
            yield return BoneShipment(burst, burst);
        }

        End();
        yield break;
    }

    //use fixed update to make conveyor movement as consistent as possible
    void FixedUpdate()
    {
        mBody.position -= Time.fixedDeltaTime * velocity;
        mBody.MovePosition(initalPos);
    }

    override protected void Awake()
    {
        base.Awake();
        mGroup = gameObject.GetComponent<conveyorGroup>();
        mBody = gameObject.GetComponent<Rigidbody>();
        initalPos = transform.position;
        foreach (var bc in gameObject.GetComponentsInChildren<BoxCollider>())
        {
            if (bc.isTrigger)
                dropCollider = bc;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        //add repeat bones to list
        for (int i = 0; i < count.Count; i++)
        {
            for (int c = 1; c < count[i]; c++)
            {
                orderedBones.Add(orderedBones[i]);
            }
        }
#if UNITY_EDITOR
        //check burst total against bone total
        int burstCount = 0;
        foreach (int i in bursts)
        {
            burstCount += i;
        }
        if (burstCount != orderedBones.Count)
        {
            Debug.LogError("Burst Total (" + burstCount + ") not equal to bone total (" + orderedBones.Count + ")");
        }
#endif
        //randomize bone order
        for (int i = 0; i < 50; i++)
        {
            int rand1 = Random.Range(0, orderedBones.Count);
            int rand2 = Random.Range(0, orderedBones.Count);
            GameObject temp = orderedBones[rand2];
            orderedBones[rand2] = orderedBones[rand1];
            orderedBones[rand1] = temp;
        }
        boneManager.NumGroups = orderedBones.Count;
    }
}
