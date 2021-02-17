using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWalk : MonoBehaviour
{
    [Header("WalkSettings")]
    [SerializeField]
    //how long steps take
    private float stepTime = default;
    [SerializeField]
    //how far from the origin limbs will get before moving
    private float footToOriginLimit = default;
    [SerializeField]
    //how fast the cat is walking
    private float walkSpeed = default;
    [SerializeField]
    private float stepHeight = default;

    [Header("Transforms")]
    [SerializeField]
    private List<Transform> feetTransforms = default;

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
        feet.Add(new FootData(feetTransforms[0], new Vector3(0, 0, -footToOriginLimit)));
        feet.Add(new FootData(feetTransforms[1], new Vector3(0, 0, footToOriginLimit * 10/15)));
        feet.Add(new FootData(feetTransforms[2], new Vector3(0, 0, -footToOriginLimit * 9 / 25)));
        feet.Add(new FootData(feetTransforms[3], new Vector3(0, 0, footToOriginLimit)));

        feet[0].stepable = true;
        feet[3].stepable = false;
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
                    if (foot.stepable)
                        StartCoroutine(foot.Step(stepTime,
                            dist + footToOriginLimit,
                            //dist + footToOriginLimit - 0.01f,
                            stepHeight
                        ));
                }
                else
                    foot.stepable = true;
            }
        }
    }
}

class FootData
{
    Transform target;
    Transform origin;
    public Vector3 pinPos;
    public bool stepping { get; private set; }
    public bool stepable { get; set; }

    public FootData(Transform target, Vector3 offset)
    {
        this.target = target;
        origin = GameObject.Instantiate(target.gameObject, target.parent).transform;
        pinPos = target.position + offset;
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
        stepable = stepping;
        yield break;
    }
}
