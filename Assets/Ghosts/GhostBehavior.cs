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
        boneLocation = transform.GetChild(2);
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
        StartCoroutine(Surprised(transform.GetChild(0),transform.GetChild(1)));
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

    #region behaviors



    IEnumerator Surprised(Transform head, Transform arms)
    {
        const float maxScaleIncrease = 1.5f;
        const float armRotation = 180;
        const float animVariation = .2f;
        float firstAnimTime = .5f * (1 + Random.value* animVariation - animVariation/2);
        float secondAnimTime = 1f * (1 + Random.value * animVariation - animVariation / 2);

        float time = 0;
        Transform eyeone = head.GetChild(0);
        Transform eyetwo = head.GetChild(1);
        Vector3 initalScale = eyeone.localScale;

        Vector3 initalRotation = arms.localEulerAngles;

        while (time < firstAnimTime)
        {
            time += Time.deltaTime;
            eyeone.localScale = initalScale * (1 + (maxScaleIncrease - 1) * time / firstAnimTime);
            eyetwo.localScale = initalScale * (1 + (maxScaleIncrease - 1) * time / firstAnimTime);

            arms.localRotation = Quaternion.Euler(initalRotation + new Vector3(0, 0, time / firstAnimTime * armRotation));

            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        while (time < secondAnimTime)
        {

            time += Time.deltaTime;
            eyeone.localScale = initalScale * (maxScaleIncrease - (maxScaleIncrease - 1) * time / secondAnimTime);
            eyetwo.localScale = initalScale * (maxScaleIncrease - (maxScaleIncrease - 1) * time / secondAnimTime);

            arms.localRotation = Quaternion.Euler(initalRotation + new Vector3(0, 0, armRotation - armRotation * time / secondAnimTime));

            yield return null;
        }

        yield break;
    }

    private void OnDestroy()
    {
        if(mBone)
        {
            mBone.mGhost = null;
        }
    }

    #endregion
}
