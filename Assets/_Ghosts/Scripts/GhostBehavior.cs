using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostBehavior : MonoBehaviour
{
    //movement values
    private Vector3 target;
    private List<Vector3> path;
    private int pathIndex;
    private Vector3 endForward;
    [SerializeField]
    private Animator animator;

    [SerializeField]
    float minorJumpMult;
    [SerializeField]
    float majorJumpMult;


    //bone values
    public GrabbableGroup mBone;
    [SerializeField]
    private Transform leftHand;
    [SerializeField]
    private Transform rightHand;
    public Vector3 BoneLocation
    {
        get
        {
            return (leftHand.position + rightHand.position) / 2;
        }
    }

    public GhostPhysics body { get; private set; }

    private void Awake()
    {
        body = gameObject.GetComponent<GhostPhysics>();
        animator.Play("Idle", 0, Random.value);
    }

    // Update is called once per frame
    void Update()
    {
        if (mBone)
        {
            mBone.transform.position = BoneLocation;
        }
    }

    private void BodyArrived()
    {
        pathIndex++;
        //end of path reached
        if (pathIndex >= path.Count)
        {
            body.RotateTo(endForward);
            // Plays sound when cats reach destination
            //AudioManager.Instance.PlayTestSound();
        }
        else
        {
            float radious = pathIndex == path.Count - 1 ? 0.05f : 0.2f;
            body.LookAt(path[pathIndex]);
            body.MoveToPosition(
                path[pathIndex],
                radious,
                pathIndex == path.Count - 1 //arrival forces for last path point
            );
        }
    }

    public void AddToPath(GameObject pathPoint, bool isFinal)
    {
        if (path == null)
        {
            path = new List<Vector3>();
        }
        path.Add(pathPoint.transform.position);
        if (isFinal)
            endForward = pathPoint.transform.forward;
    }

    public void FollowPath()
    {
        pathIndex = -1;
        BodyArrived();
        body.ArrivalCallback = BodyArrived;
    }

    public void LostBone()
    {
        mBone.mGhost = null;
        mBone = null;
    }

    public void Shock(bool isMinor)
    {
        if (isMinor)
            MinorShock();
        else
            MajorShock();
    }

    public void MinorShock()
    {
        Debug.Log("Minor shock");

        animator.SetTrigger("minorShockTrigger");
        body.Jump(minorJumpMult);
        //AudioManager.Instance.PlayMinorShock();
    }

    public void MajorShock()
    {
        Debug.Log("Major shock");

        //animate
        animator.SetTrigger("majorShockTrigger");
        body.Jump(majorJumpMult);

        //play audio before potential return from bone throwing
        AudioManager.Instance.PlayMajorShock();

        //throw bone 
        if (!mBone)
            return;
        var boneRb = mBone.Rb;
        mBone.PickedUp();
        boneRb.useGravity = true;
        body.Jump(2 * majorJumpMult, boneRb);

        Recall();
    }

    public void Recall(float lifeSpan = 3)
    {
        IEnumerator RecallRoutine()
        {
            body.MoveToPosition(path[0],0);
            yield return new WaitForSeconds(lifeSpan);
            GhostManager.Instance.DestroyGhost(this);
            yield break;
        }
        StartCoroutine(RecallRoutine());
    }

    private void OnCollisionEnter(Collision collision)
    {
        GhostManager.Collision.AddCollision(this, collision);
    }

    private void OnDestroy()
    {
        if(mBone)
        {
            mBone.mGhost = null;
        }
    }

}
