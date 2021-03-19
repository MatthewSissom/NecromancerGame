using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbEnd : MonoBehaviour
{
    //---Enums---//

    //Describes what type of animation this leg should use
    public enum LimbTag
    {
        Pair,   //There two legs on this half of the body (two front or two back)
        Single, //There is only one leg on this half of the body
        Spine,  //There are are no legs on the back part of the body
        Worm,   //There are no legs anywhere on the body
    }

    //Where this limb starts on the body
    public enum LimbLocationTag
    {
        FrontLeft,
        FrontRight,
        BackLeft,
        BackRight,
    }

    //What this limb is currently doing
    public enum LimbStates
    {
        Stepping,
        Pushing,
        Standing
    }


    //---Public Felids---//
    public float StepSpeed;
    public float StepHeight;

    //A combination of where this limb is on the body and how it should move
    [field: SerializeField]
    public LimbTag Type { get; private set; }
    //where on the body the limb is located
    [field: SerializeField]
    public LimbLocationTag LocationTag { get; private set; }
    //The state the limb is currently in
    public LimbStates LimbState { get; private set; }
    [field: SerializeField]
    //how long the limb is when fully extended
    public float LimbLength { get; private set; }
    [field: SerializeField]
    //the diamater of the circle that the limb can trace on the ground
    public float StrideLength { get; private set; }
    [field: SerializeField]
    //The gameobject that this limb will orient to
    public GameObject Target { get; private set; }
    //the gameobject that marks the start of the limb
    [field: SerializeField]
    public GameObject LimbStart { get; private set; }
    //a percentage from 0 to 1 of how extended the limb is
    public float Extension { get { return (Target.transform.position - LimbStart.transform.position).magnitude / LimbLength; } }

    //---Public Events---//

    //delegates
    public delegate void StepStartDelegate(LimbEnd callingLimb, Vector3 target);
    public delegate void StepEndDelegate(LimbEnd callingLimb, Vector3? collisionPoint);
    public delegate void LimbEventDelegate(LimbEnd callingLimb);

    //Envoked when a limb moves off the ground with the intention of stepping
    //Step end is not gaurenteed to be called after step start
    public event LimbEventDelegate StepStartEvent;
    //Envoked when a limb finishes stepping
    public event StepEndDelegate StepEndEvent;


    //---Private Feilds---//

    private Coroutine currentRoutine;
    private Vector3 stepTargetPos;

    public void SetStepTarget(Vector3 mTarget)
    {
        stepTargetPos = mTarget;
    }

    public void TempLimbInit(float groundHeight)
    {
        LimbState = LimbStates.Standing;
        StrideLength = Mathf.Sqrt(LimbLength * LimbLength + Mathf.Pow(LimbStart.transform.position.y - groundHeight, 2));
        StartPush();
    }

    public void LimbInit(LimbTag type, LimbLocationTag limbLocation, float length, float strideLength, GameObject target, GameObject limbStart)
    {
        Type = type;
        LocationTag = limbLocation;

        LimbLength = length;
        StrideLength = strideLength;
        Target = target;
        LimbStart = limbStart;
    }

    //can be called on a grounded limb to start pushing the body forward
    public void StartPush()
    {
        if(LimbState != LimbStates.Standing)
        {
            Debug.LogError("Limb is stepping or alread pushing, cannot push");
            return;
        }

        LimbState = LimbStates.Pushing;
        currentRoutine =  StartCoroutine(PushRoutine());
    }

    //moves a limb to a target
    public void StartStep()
    {
        if (LimbState == LimbStates.Pushing)
        {
            EndPush();
        }

        StepStartEvent?.Invoke(this);
        LimbState = LimbStates.Stepping;
        currentRoutine = StartCoroutine(StepRoutine());
    }

    public void Destableized()
    {
        if (LimbState == LimbStates.Pushing)
        {
            EndPush();
        }
    }

    public void Collided(Vector3 collisionPoint)
    {
        if (LimbState == LimbStates.Stepping)
        {
            StepEnd(collisionPoint);
        }
    }

    //called when a limb reaches it's maximum extent
    private void EndPush()
    {
        //check to see if end push was not called from the push coroutine
        if (LimbState == LimbStates.Pushing)
        {
            StopCoroutine(currentRoutine);
            LimbState = LimbStates.Standing;
        }
    }

    //called when a foot is placed on the ground after a step
    private void StepEnd(Vector3? collisonPoint)
    {
        if (LimbState == LimbStates.Stepping)
        {
            LimbState = LimbStates.Standing;
            StopCoroutine(currentRoutine);
        }
        StepEndEvent?.Invoke(this, collisonPoint);
    }

    private IEnumerator StepRoutine()
    {
        //final position of foot relitive to the origin
        Vector3 inital = Target.transform.position;
        Debug.Log("new step");
        float elapsedTime = 0;
        float percentFinished = 0;
        float stepTime = (stepTargetPos - inital).magnitude / StepSpeed;
        while (percentFinished < 1)
        {
            elapsedTime += Time.deltaTime;
            percentFinished = elapsedTime / stepTime;
            Target.transform.position = Vector3.Lerp(inital, stepTargetPos, percentFinished)
                + new Vector3(0, Mathf.Sin(percentFinished * Mathf.PI) * StepHeight, 0);
            yield return null;
        }
        Target.transform.position = stepTargetPos;
        StepEnd(null);
        yield break;
    }

    private IEnumerator PushRoutine()
    {
        Transform targetTransfrom = Target.transform;
        Vector3 groundedTargetPosition = Target.transform.position;
        bool contracting = true;
        while (contracting || Extension < .7 )
        {
            if (contracting)
                contracting = !((Extension < .6) || Extension > 1);

            targetTransfrom.position = groundedTargetPosition;
            yield return null;
        }
        LimbState = LimbStates.Standing;
        StartStep();
    }
}