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
    private Collider collider;
    private Rigidbody rb;

    void Awake()
    {
        collider = GetComponent<Collider>();
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
        collider.enabled = false;
        rb.isKinematic = true;
    }

    private void EndApproach()
    {
        approaching = false;
        collider.enabled = true;
        rb.isKinematic = false;
    }
}
