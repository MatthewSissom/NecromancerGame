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

        if (initalPos == pos && forward == transform.forward && up == transform.up)
        {
            End();
            yield break;
        }

        initalTime = Time.time;
        initalQuat = transform.rotation;
        Quaternion endQuat = Quaternion.LookRotation(forward, up);
        elapsedTime = 0;

        yield return null;

        while(elapsedTime < time)
        {
            transform.localPosition = Vector3.Lerp(initalPos, pos, elapsedTime / time);
            transform.rotation = Quaternion.Slerp(initalQuat, endQuat, elapsedTime / time);
            elapsedTime = Time.time - initalTime;
            yield return null;
        }

        // WILL - Plays a chalk sound effect when the board flips and the menu transitions
        AudioManager.Instance.PlayChalkboardSFX();

        transform.localPosition = pos;
        transform.rotation = endQuat;

        End();
        yield break;
    }

    public override void SetName()
    {
        Name = transitionName;
    }

    protected override void Awake()
    {
        base.Awake();
        forward.Normalize();
    }
}
