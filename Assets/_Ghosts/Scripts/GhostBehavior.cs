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

    //bone values
    public Bone mBone;
    public Transform boneLocation { get; private set; }

    public GhostPhysics body { get; private set; }

    private void Awake()
    {
        body = gameObject.GetComponent<GhostPhysics>();
        boneLocation = transform.Find("BoneLocation");
    }

    // Update is called once per frame
    void Update()
    {
        if (mBone)
        {
            mBone.transform.position = boneLocation.position;
        }
    }

    private void BodyArrived()
    {
        pathIndex++;
        //end of path reached
        if(pathIndex >= path.Count)
        {
            body.RotateTo(endForward);
        }
        else
        {
            float radious = pathIndex == path.Count - 1? 0.05f:0.2f;
            body.LookAt(path[pathIndex]);
            body.MoveToPosition(
                path[pathIndex],
                radious,
                pathIndex == path.Count-1 //arrival forces for last path point
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
        mBone = null;
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


    private void OnDestroy()
    {
        if(mBone)
        {
            mBone.mGhost = null;
        }
    }

}
