using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointApproacher : MonoBehaviour
{
    private Vector3 startPoint;
    private Vector3 target;

    private bool approaching;
    public bool Approaching
    {
        get
        {
            return approaching;
        }
    }

    private float approachTimer;
    private float approachTimerMax;
    private GrabbableGroup bone;
    private Rigidbody rb;

    void Awake()
    {
        bone = GetComponent<GrabbableGroup>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(approaching)
        {
            approachTimer += Time.deltaTime;
            if (approachTimer > approachTimerMax)
            {
                approachTimer = approachTimerMax;
            }
            transform.position = Vector3.Lerp(startPoint, target, Mathf.Sqrt(approachTimer / approachTimerMax));

            if(approachTimer == approachTimerMax)
            {
                EndApproach();
            }
        }
    }

    public void StartApproach(Vector3 targetPos, float time)
    {
        approachTimer = 0;
        approachTimerMax = time;
        startPoint = transform.position;
        target = targetPos;
        approaching = true;
        bone.FullLayerChange(bone.gameObject, 14);
        rb.isKinematic = true;
    }

    private void EndApproach()
    {
        approaching = false;
        bone.FullLayerChange(bone.gameObject, 10);
        rb.isKinematic = false;
    }
}
