using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTouch : TouchProxy
{
    //rotation can either be around the up axis (when the player spins a bone)
    //or around the vector perpendicular to up and toParent (when a player pinches a bone)
    float angleDistAroundUp = 0;
    float stopWatchAngle = 0;
    float linearDistAroundUp = 0;
    Vector3 toParent;
    Vector3 toParentPerp;
    //which axis is the player currently rotating around?
    private bool aroundUp = true;

    //game feel values
    [SerializeField]
    float rotForceMulti = 1;
    [SerializeField]
    float switchResistance = 0;
    [SerializeField]
    float aVelocityClamp = 1.0f;
    [SerializeField]
    float stopWatchRotModifier = 0.5f;
    [SerializeField]
    float linearDistMulti= 10;
    
   

    
    public Vector3 realUp;

    //Score - tracks if the player is trying to spin or pinch
    const int scoresLength = 5;
    //holds the scores from the last few frames to avoid switching due to an outlier
    float[] scores = new float[scoresLength];
    int scoreIndex = 0;
    //the sum of everything in scores
    float currentScore;

    //Up vector in relation to camera not world space

    private BoneMovingTouch parent;
    public BoneMovingTouch Parent
    {
        get { return parent; }
        set
        {
            parent = value;
            //replace this touch with a functionless one if the floating touch is removed
            parent.DisableEvent += () => { InputManager.Instance.DisableTouch(this); };
        }
    }

    public void ResetTouch(Vector3 pos, float rad)
    {
        base.Move(pos, rad);
        toParent = (parent.transform.position - transform.position).normalized;

        if (parent.activeBone != null||parent.activeWatch != null)
        {
            parent.CancleStopRotation();
        }

        angleDistAroundUp = 0;
        stopWatchAngle = 0;

        currentScore = 0;
        for (int i = 0; i < scoresLength; i++)
        {
            scores[i] = 0;
        }
    }

    public override void Move(Vector3 pos, float rad)
    {
        //store old values
        Vector3 oldToParent = toParent;
        Vector3 oldPos = transform.position;

        //update position
        base.Move(pos, rad);
        toParent = (parent.transform.position - transform.position).normalized;

        //update scores and check for axis change//

        //score goes from -.5 to .5 based on if the movement is in the 
        //direction of the parent (.5) or perpendicular to it (-.5)
        Vector3 movementVector = pos - oldPos;
        //split the vector into components
       

        //adjust distances
     
        angleDistAroundUp = Vector3.SignedAngle(oldToParent, toParent, realUp);
        stopWatchAngle = angleDistAroundUp;
        //Debug.Log(Mathf.Sign(angleDistAroundUp));
        linearDistAroundUp += Vector3.Distance(oldToParent, toParent) * linearDistMulti * Mathf.Sign(angleDistAroundUp);


    }

    protected void Update()
    {
        if (parent.activeWatch == null&&parent.activeBone == null)
            return;

        if (parent.activeWatch != null)
        {
            parent.activeWatch.ChangeAngle(-angleDistAroundUp * stopWatchRotModifier);
            if (angleDistAroundUp < 0)
                angleDistAroundUp = 0;
            return;
        }
        //calculate angular velocities around axies
        Vector3 aVelocity = Vector3.zero;

        float time = Time.deltaTime;

        float lVelocity = linearDistAroundUp * rotForceMulti*time;
        linearDistAroundUp -= lVelocity * time;

        aVelocity = lVelocity * realUp;

        parent.activeBone.Rb.angularVelocity = aVelocity;
       

    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (parent && parent.isActiveAndEnabled)
            parent.StopRotation(rotForceMulti * Mathf.Deg2Rad);
    }
}
