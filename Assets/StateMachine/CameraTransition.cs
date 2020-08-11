using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransition : State
{
    public string transitionName;
    public float time;
    public Vector3 pos;
    public Vector3 forward;
    public Vector3 up;

    private Vector3 axis;
    private Vector3 initalPos;
    private Vector3 initalForward;
    private Quaternion initalQuat;
    private float initalTime;
    private float elapsedTime;
    private float degrees;

    public override IEnumerator Routine()
    {
        Begin();

        initalPos = transform.localPosition;
        initalForward = transform.forward;

        if (initalPos == pos && initalForward == forward)
        {
            End();
            yield break;
        }

        initalTime = Time.time;
        initalQuat = transform.rotation;
        elapsedTime = 0;
        float dot = Vector3.Dot(initalForward, forward);
        if (Mathf.Abs(dot) == 1)
        {
            axis = new Vector3(1, 0, 0);
            degrees = 180;
        }
        else
        {
            axis = Vector3.Cross(initalForward, forward);
            degrees = Mathf.Acos(dot) / Mathf.PI * 180;
        }
        //positive or negitive rotation?


        yield return null;

        while(elapsedTime < time)
        {
            transform.localPosition = Vector3.Lerp(initalPos, pos, elapsedTime / time);
            transform.rotation = initalQuat;
            transform.Rotate(axis, degrees * elapsedTime / time);
            elapsedTime = Time.time - initalTime;
            yield return null;
        }

        transform.localPosition = pos;
        transform.up = up;

        End();
        yield break;
    }

    protected override void Awake()
    {
        Name = transitionName;
        forward.Normalize();
    }
}
