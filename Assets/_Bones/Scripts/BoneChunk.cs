using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneChunk : MonoBehaviour
{
    public BoneType boneType;
    public ThemeType themeType;

    public enum BoneType
    { 
        BackSpine,
        FRightLeg,
        LowerTail,
        MidSpine,
        Neck,
        RightBack,
        Spine,
        Lower1,
        Lower
    }

    public enum ThemeType
    {
        Normal,
        Tea
    }
}
