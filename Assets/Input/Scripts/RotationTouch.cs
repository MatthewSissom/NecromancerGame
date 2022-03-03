using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTouch : TouchProxy
{
    //rotation can either be around the up axis (when the player spins a bone)
    //or around the vector perpendicular to up and toParent (when a player pinches a bone)
    float angleDistAroundUp = 0;
    float angleDistAroundParent = 0;
    Vector3 toParent;
    Vector3 toParentPerp;
    //which axis is the player currently rotating around?
    private bool aroundUp = true;

    //game feel values
    [SerializeField]
    float aroundToParentMult = 1000;
    [SerializeField]
    float acceleration = 1000;
    [SerializeField]
    float switchResistance = 0;
    [SerializeField]
    float aVelocityClamp = 1.0f;

    //Score - tracks if the player is trying to spin or pinch
    const int scoresLength = 5;
    //holds the scores from the last few frames to avoid switching due to an outlier
    float[] scores = new float[scoresLength];
    int scoreIndex = 0;
    //the sum of everything in scores
    float currentScore;

    //Up vector in relation to camera not world space
    Vector3 realUp;

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

        if (parent.activeWatch != null)
        {
            parent.CancleStopRotation();
        }

        angleDistAroundUp = 0;
        angleDistAroundParent = 0;

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
        
        angleDistAroundUp += Vector3.SignedAngle(oldToParent, toParent, Vector3.up);
        
    }

    protected void Update()
    {
        if (parent.activeWatch == null && parent.activeBone == null)
            return;

        //calculate angular velocities around axies
        GrabbableGroup bone = parent.activeBone;
        Vector3 aVelocity = bone.Rb.angularVelocity;
        Vector3 directionality;

        if (bone.RightForward)
        {
            directionality = new Vector3(1.000f, 0, 0);
        }
        else
        {
            directionality = new Vector3(0, 0, -1.000f);
        }

        directionality *= bone.FlippedMultiplier;
           

        //magnitude of the projection onto a normal is the dot product
        float velocityAroundUp = Vector3.Dot(aVelocity, realUp);
        

        //adjust distances
        float time = Time.deltaTime;
        
        angleDistAroundUp -= time * velocityAroundUp * Mathf.Rad2Deg;

        //calculate velocity corrections
        velocityAroundUp = Mathf.Sign(angleDistAroundUp) * Mathf.Sqrt(2 * Mathf.Abs(angleDistAroundUp) * acceleration) * Mathf.Deg2Rad - velocityAroundUp;
        
        //apply corrections
        aVelocity += velocityAroundUp * directionality;


        //remove any rotation along toParent, it is unwanted
        //aVelocity -= Vector3.Dot(aVelocity, toParent) * toParentPerp;

        if (aVelocity.magnitude < aVelocityClamp)
        {
            aVelocity = Vector3.zero;
        }

        //push calculated value to the rigidbody
        bone.Rb.angularVelocity = aVelocity;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (parent && parent.isActiveAndEnabled)
            parent.StopRotation(acceleration * Mathf.Deg2Rad);
    }
}
