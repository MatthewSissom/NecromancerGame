using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransition : State
{
    public string transitionName;
    public Vector3 pos;
    public Vector3 rotation;
    public float time;

    private Vector3 initalPos;
    private Vector3 initalRotation;
    private float initalTime;
    private float elapsedTime;

    public override IEnumerator Routine()
    {
        Begin();

        initalPos = gameObject.transform.position;
        initalRotation = gameObject.transform.rotation.eulerAngles;
        initalTime = Time.time;
        elapsedTime = 0;

        yield return null;

        while(elapsedTime < time)
        {
            gameObject.transform.position = Vector3.Lerp(initalPos, pos, elapsedTime / time);
            gameObject.transform.rotation = Quaternion.Euler(Vector3.Lerp(initalRotation, rotation, elapsedTime / time));
            elapsedTime = Time.time - initalTime;
            yield return null;
        }

        gameObject.transform.position = pos;
        gameObject.transform.rotation = Quaternion.Euler(rotation);

        End();
        yield break;
    }

    private void Awake()
    {
        Name = transitionName;
    }
}
