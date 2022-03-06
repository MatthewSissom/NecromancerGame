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
        Stump,
        StumpSingle
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
        Standing,
        Jumping
    }


    //---Public Felids---//
    public float StepSpeed;
    [field: SerializeField]
    public float StepHeight { get; set; }
    public float HeightOffset { get; private set; }
    public float BodyMovementMult = .015f;

    //A combination of where this limb is on the body and how it should move
    [field: SerializeField]
    public LimbTag Type { get; private set; }
    //where on the body the limb is located
    [field: SerializeField]
    public LimbLocationTag LocationTag { get; private set; }
    [field: SerializeField]
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
    [field: SerializeField]
    public int DelayIndex { get; private set; }

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
    private float expectedPushTime;

    public void SetStepTarget(Vector3 mTarget, float expectedPushTime)
    {
        stepTargetPos = mTarget;
        this.expectedPushTime = expectedPushTime;
    }

    public void DefaultStepTarget()
    {
        stepTargetPos = transform.position;
        expectedPushTime = 0.25f;
    }

    public void LimbInit(float length, GameObject target, GameObject limbStart)
    {
        LimbLength = length;
        Target = target;
        LimbStart = limbStart;
        LimbState = LimbStates.Standing;
    }

    public void SetTags(LimbTag type, LimbLocationTag limbLocation, int delayIndex)
    {
        Type = type;
        LocationTag = limbLocation;
        DelayIndex = delayIndex;
    }

    //get lenght of distance the limb will spend on ground based on length (hypotenuse) and distance from ground
    public void SetStride(float chestDistFromGround)
    {
        //avoid irrational and 0 solutions
        if (LimbLength <= chestDistFromGround)
            StrideLength = LimbLength;
        else
            StrideLength = Mathf.Sqrt(LimbLength * LimbLength - chestDistFromGround  * chestDistFromGround);

        if (float.IsNaN(StrideLength))
            Debug.Log("NAN length!");
    }

    //can be called on a grounded limb to start pushing the body forward
    public void StartPush()
    {
        if(LimbState != LimbStates.Standing)
        {
            StartStep();
            return;
        }

        LimbState = LimbStates.Pushing;

        if (expectedPushTime == 0)
            expectedPushTime = 0.25f;
        currentRoutine =  StartCoroutine(PushRoutine());
    }

    //moves a limb to a target
    public void StartStep()
    {
        if (LimbState == LimbStates.Pushing)
        {
            EndPush();
        }
        if(LimbState == LimbStates.Jumping)
        {
            Debug.LogError("Trying to step during jump");
        }

        StepStartEvent?.Invoke(this);
        LimbState = LimbStates.Stepping;
        currentRoutine = StartCoroutine(StepRoutine());
    }

    public void StartJump(JumpArc jumpArc)
    {
        if(currentRoutine != null)
            StopCoroutine(currentRoutine);
        LimbState = LimbStates.Jumping;

    }

    public void EndJump()
    {
        if(LimbState == LimbStates.Jumping && currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }
        LimbState = LimbStates.Standing;
    }

    public void PathStarted()
    {
        if (LimbState == LimbStates.Jumping)
            EndJump();
        //if already moving, no change
        else if (LimbState != LimbStates.Standing)
            return;

        //when a path first starts only half of limbs will step,
        //hopefully giving cats a more natural gait
        switch (LocationTag)
        {
            case LimbLocationTag.FrontLeft:
                StartPush();
                break;
            case LimbLocationTag.FrontRight:
                StartStep();
                break;
            case LimbLocationTag.BackLeft:
                StartStep();
                break;
            case LimbLocationTag.BackRight:
                StartPush();
                break;
            default:
                break;
        }
    }

    //called when a limb reaches it's maximum extent
    private void EndPush()
    {
        //check to see if end push was not called from the push coroutine
        if (LimbState == LimbStates.Pushing && currentRoutine != null)
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
            if(currentRoutine != null)
                StopCoroutine(currentRoutine);
        }
        StepEndEvent?.Invoke(this, collisonPoint);
    }

    private IEnumerator StepRoutine()
    {
        //final position of foot relitive to the origin
        Vector3 inital = Target.transform.position;
        float elapsedTime = 0;
        float percentFinished = 0;
        float stepTime = (stepTargetPos - inital).magnitude / StepSpeed;
        if(stepTime == 0)
        {
            StepEnd(null);
            yield break;
        }
        float initalHOffset = HeightOffset;
        while (percentFinished < 1)
        {
            elapsedTime += Time.deltaTime;
            percentFinished = elapsedTime / stepTime;

            HeightOffset = initalHOffset * (1 - percentFinished);

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
        float timer = 0;
        while (timer < expectedPushTime || Extension < 1 )
        {
            timer += Time.deltaTime;

            HeightOffset = Mathf.Sin(timer / expectedPushTime * Mathf.PI) * BodyMovementMult * LimbLength;

            targetTransfrom.position = groundedTargetPosition;
            yield return null;
        }
        LimbState = LimbStates.Standing;
        StartStep();
    }

    void LimbEndedStep(LimbEnd calling, Vector3? collisionPoint)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Cats/Footsteps/SkeletonFootsteps");
        calling.StartPush();
    }
}