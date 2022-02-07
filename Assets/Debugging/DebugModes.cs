using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugModes : MonoBehaviour
{
#if UNITY_EDITOR
    public enum EStateDebugMode
    {
        None,
        SkipMenus,
        PlaypenOnly
    }
    [SerializeField]
    EStateDebugMode stateMode;

    [Header("Input")]
    [SerializeField]
    bool toggleMouseInput;

    [Header("IK")]
    [SerializeField]
    GameObject ikTestPrefab;

    public static EStateDebugMode StateMode { get { return instance.stateMode; } }
    public static bool UseMouseInput { get { return instance.toggleMouseInput; } }
    public static GameObject IKTestPrefab { get { return instance.ikTestPrefab; } }


    private static DebugModes instance;
    private void Awake()
    {
        if (instance)
            Destroy(this);
        else
            instance = this;
    }
#endif
}
