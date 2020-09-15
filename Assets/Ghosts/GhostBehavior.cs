using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostBehavior : MonoBehaviour
{
    public Vector3 destination;
    public float timeToDest;
    public bone mBone;
    private Transform boneLocation;
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
        if(timeToDest > 0)
        {
            float inital = timeToDest;
            timeToDest -= Time.deltaTime;
            transform.position += (destination - transform.position) * (inital - timeToDest) / inital;
        }
    }

    public void LostBone()
    {
        IEnumerator surprise(Transform head, Transform arms)
        {
            const float maxScaleIncrease = 1.5f;
            const float armRotation = 180;
            const float firstAnimTime = .5f;
            const float secondAnimTime = 1f;

            float time = 0;
            Transform eyeone = head.GetChild(0);
            Transform eyetwo = head.GetChild(1);
            Vector3 initalScale = eyeone.localScale;

            Vector3 initalRotation = arms.localEulerAngles;

            while(time < firstAnimTime)
            {
                time += Time.deltaTime;
                eyeone.localScale = initalScale * (1 + (maxScaleIncrease - 1) * time / firstAnimTime);
                eyetwo.localScale = initalScale * (1 + (maxScaleIncrease - 1) * time / firstAnimTime);

                arms.localRotation = Quaternion.Euler(initalRotation + new Vector3(0, 0, time / firstAnimTime * armRotation));

                yield return null;
            }

            yield return new WaitForSeconds(0.3f);

            while(time < secondAnimTime)
            {

                time += Time.deltaTime;
                eyeone.localScale = initalScale * (maxScaleIncrease - (maxScaleIncrease -1)*time/secondAnimTime);
                eyetwo.localScale = initalScale * (maxScaleIncrease - (maxScaleIncrease - 1) * time / secondAnimTime);

                arms.localRotation = Quaternion.Euler(initalRotation + new Vector3(0, 0, armRotation - armRotation*time/secondAnimTime));

                yield return null;
            }

            yield break;
        }
        StartCoroutine(surprise(transform.GetChild(0),transform.GetChild(1)));

        mBone = null;
    }
}
