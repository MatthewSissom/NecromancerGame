using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostBehavior : MonoBehaviour
{
    //movement values
    private Vector3 destination;

    //bone values
    public Bone mBone;
    private Transform boneLocation;

    public GhostPhysics body { get; private set; }

    private void Awake()
    {
        body = gameObject.GetComponent<GhostPhysics>();        
    }

    // Start is called before the first frame update
    void Start()
    {
        mBone.mGhost = this;
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

    public void LostBone()
    {
        mBone = null;
    }

    public void Recall()
    {
        IEnumerator RecallRoutine()
        {
            body.MoveToPosition(transform.position + new Vector3(0, 0, 1));
            yield return new WaitForSeconds(3);
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
