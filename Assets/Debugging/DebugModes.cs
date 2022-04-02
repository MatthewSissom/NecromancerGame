using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugModes : MonoBehaviour
{
#if UNITY_EDITOR
    public enum EStateDebugMode
    {
        None,
        Tutorial,
        SkipMenus,
        AssemblyAndPlayPen,
        PlaypenOnly
    }
    [SerializeField]
    EStateDebugMode stateMode;

    [SerializeField]
    bool skipCameraTransitions;

    [Header("Input")]
    [SerializeField]
    bool toggleMouseInput;

    [Header("Playpen")]
    [SerializeField]
    GameObject ikTestPrefab;

    public enum DebugPathFlags
    {
        None = 0,
        NavMeshPath = 1,
        ModifiedNavMeshPath= 2,
        TruePath = 4,
        All = 8 - 1
    }
    [SerializeField]
    DebugPathFlags skeletonPathMode;

    public static EStateDebugMode StateMode { get { return instance.stateMode; } }
    public static bool SkipCameraTransitions { get { return instance.skipCameraTransitions; } }
    public static bool UseMouseInput { get { return instance.toggleMouseInput; } }
    public static GameObject IKTestPrefab { get { return instance.ikTestPrefab; } }
    public static DebugPathFlags SkeletonPathMode { get { return instance.skeletonPathMode; } }


    private static DebugModes instance;
    private DebugPathFlags previousPathFlag;

    private void Awake()
    {
        if (instance)
            Destroy(this);
        else
            instance = this;

        previousPathFlag = SkeletonPathMode;
    }

    private void Update()
    {
        // track changes to path debug
        if (previousPathFlag != SkeletonPathMode)
        {
            previousPathFlag = SkeletonPathMode;
            DebugRendering.RenderModeChanged();
        }
    }
#endif
}
