using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbEnd : MonoBehaviour
{
    //TEMP for calculating length (should be done by assembler)
    [SerializeField]
    BoneAxis boneAxisDict = default;

    //---Enums---//
    public enum LimbTag
    {
        FrontRight,
        FrontLeft,
        BackRight,
        BackLeft,
        SingleFront,
        SingleBack,
        SpineLimb
    }
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
    //The state the limb is currently in
    public LimbStates LimbState { get; private set; }
    //how long the limb is when fully extended
    [field: SerializeField]
    public float Length { get; private set; }
    //The gameobject that this limb will orient to
    [field: SerializeField]
    public GameObject Target { get; private set; }
    //the gameobject that marks the start of the limb
    [field: SerializeField]
    public GameObject LimbStart { get; private set; }
    //a percentage from 0 to 1 of how extended the limb is
    public float Extension { get { return (Target.transform.position - LimbStart.transform.position).magnitude / Length; } }

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
    float chestHeight;

    public void TempLimbInit(float chestHeight)
    {
        LimbState = LimbStates.Standing;
        this.chestHeight = chestHeight;
    }

    public void LimbInit(LimbTag type, float length, GameObject target, GameObject limbStart)
    {
        Type = type;
        Length = length;
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
    public void StartStep(Vector3 target)
    {
        if (LimbState == LimbStates.Pushing)
        {
            EndPush();
        }

        LimbState = LimbStates.Stepping;
        StepStartEvent?.Invoke(this);
        currentRoutine = StartCoroutine(StepRoutine(target));
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

    private IEnumerator StepRoutine(Vector3 endPosition)
    {
        //final position of foot relitive to the origin
        Vector3 inital = Target.transform.position;

        float elapsedTime = 0;
        float percentFinished = 0;
        float stepTime = (endPosition - inital).magnitude / StepSpeed;
        while (true)
        //while (percentFinished < 1.2f)
        {
            elapsedTime += Time.deltaTime;
            percentFinished = elapsedTime / stepTime;
            Target.transform.position = Vector3.Lerp(inital, endPosition, percentFinished)
                + new Vector3(0, Mathf.Sin(percentFinished * Mathf.PI) * StepHeight, 0);
            yield return null;
        }

        Debug.Log(percentFinished);
        StepEnd(null);
        yield break;
    }

    private IEnumerator PushRoutine()
    {
        Transform targetTransfrom = Target.transform;
        Vector3 groundedTargetPosition = Target.transform.position;
        bool contracting = true;
        while (contracting || Extension < .8)
        {
            if (contracting)
                contracting = !(Extension < .7);
            Vector3 newPosition = targetTransfrom.position;
            newPosition.x = groundedTargetPosition.x;
            newPosition.z = groundedTargetPosition.z;
            newPosition.y = LimbStart.transform.position.y - chestHeight;
            targetTransfrom.position = newPosition;
            yield return null;
        }
        LimbState = LimbStates.Standing;
    }

}