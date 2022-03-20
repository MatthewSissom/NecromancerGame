using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class BasicLimbData
{
    public bool IsStump { get; private set; }
    public bool IsSingle { get; private set; }
    public bool IsFront { get; private set; }
}
public class LimbData : MonoBehaviour, IDelayedTracerData
{
    //---Enums---//

    //Describes what type of animation this leg should use



    //---Public Felids---//
    public float StepSpeed;
    public float StepHeight { get; private set; }

    //A combination of where this limb is on the body and how it should move
    [field: SerializeField]
    public LimbTag Type { get; private set; }
    //where on the body the limb is located
    [field: SerializeField]
    public LimbLocationTag LocationTag { get; private set; }
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

    [field: SerializeField]
    public int DelayIndex { get; private set; }

    public Transform Transform { get => Target.transform; }

    public float Delay { get; private set; }

    public void LimbInit(float length, GameObject target, GameObject limbStart)
    {
        LimbLength = length;
        Target = target;
        LimbStart = limbStart;
    }

    public void SetTags(LimbTag type, LimbLocationTag limbLocation)
    {
        Type = type;
        LocationTag = limbLocation;
    }

    public void SetDelay(float delay)
    {
        Delay = delay;
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
}