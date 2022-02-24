using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SkeletonBasePath
{
    public bool IsValid { get { return path != null && path.Count != 0 && path.First.Value != null; } }
    //holds all the components in the current path


    // if cat can't change paths immeditely (jumping) queue it instead
    public Vector3? queuedDestination = null;

    //---events---//
    public event System.Action PathFinished;
    public event System.Action PathStarted;
    public event System.Action PathReset;
    public event System.Action<float> ChestHeightChange;
    public event System.Action<Jump> JumpStarted;

    public SkeletonBasePath(float[] delays, Transform[] transforms, int shoulderIndex)
    {
        //default values for pathfinding settings
        MinTurningRad = .1f;
        Speed = .1f;
        this.delays = delays;

        int hipIndex = System.Math.Max(shoulderIndex - 1, 0); 
        hipDelay = delays[hipIndex];

        this.transforms = transforms;
        this.shoulderIndex = shoulderIndex;
        shoulderTransform = transforms[shoulderIndex];

        PathFinished += () => { queuedDestination = null; };
    }
}
