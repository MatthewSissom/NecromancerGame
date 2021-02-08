using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWalk : MonoBehaviour
{
    [Header("WalkSettings")]
    [SerializeField]
    //how long steps take
    private float stepTime;
    [SerializeField]
    //how far from the origin limbs will get before moving
    private float footToOriginLimit;
    [SerializeField]
    //how fast the cat is walking
    private float walkSpeed;
    [SerializeField]
    private float stepHeight;

    [Header("Transforms")]
    [SerializeField]
    private List<Transform> feetTransforms;

    private List<FootData> feet;

    void Start()
    {
        //limit walk speed based on how fast the feet can move
        if(footToOriginLimit/stepTime < walkSpeed)
        {
            walkSpeed = footToOriginLimit / stepTime;
        }

        //create feet
        feet = new List<FootData>();
        foreach (var foot in feetTransforms)
        {
            feet.Add(new FootData(foot,footToOriginLimit));
        }
    }

    // Update is called once per frame
    void Update()
    {
        //move body forwards
        Vector3 distMoved = new Vector3(0, 0, walkSpeed * Time.deltaTime);
        transform.position += distMoved;

        //update feet to check for step
        foreach (var foot in feet)
        {
            if(!foot.stepping)
            {
                foot.Pin();
                float dist = foot.GetDistFromOrigin();
                if (dist > footToOriginLimit)
                {
                    StartCoroutine(foot.Step(stepTime,
                        dist + footToOriginLimit - 0.01f,
                        stepHeight
                    ));
                }
            }
        }
    }
}

class FootData
{
    Transform target;
    Transform origin;
    Vector3 pinPos;
    public bool stepping { get; private set; }

    public FootData(Transform target, float randRange)
    {
        this.target = target;
        origin = GameObject.Instantiate(target.gameObject, target.parent).transform;
        target.transform.position += new Vector3(0, 0, Random.Range(-randRange, randRange));
        pinPos = target.position;
    }

    public float GetDistFromOrigin()
    {
        return (target.position - origin.position).magnitude;
    }

    //pin the foot to a point in world space
    public void Pin()
    {
        target.position = pinPos;
    }

    public IEnumerator Step(float stepTime, float stepDistance, float stepHeight)
    {
        stepping = true;
        //final position of foot relitive to the origin
        Vector3 inital = target.localPosition;
        Vector3 endPos = new Vector3(0, 0, stepDistance) + inital;

        float elapsedTime = 0;
        float percentFinished = 0;
        while (percentFinished < 1)
        {
            elapsedTime += Time.deltaTime;
            percentFinished = elapsedTime / stepTime;
            target.localPosition = Vector3.Lerp(inital, endPos, percentFinished)
                + new Vector3(0, Mathf.Sin(percentFinished * Mathf.PI) * stepHeight, 0);
            yield return null;
        }

        pinPos = target.position;
        stepping = false;
        yield break;
    }
}
