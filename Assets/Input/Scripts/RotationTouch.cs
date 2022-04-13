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
    [SerializeField]
    float stopWatchRotModifier = 0.5f;
    [SerializeField]
    float angleDistMulti= 0.1f;

    
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
        toParentPerp = Vector3.Cross(Vector3.up, toParent).normalized;
     
        Vector3 movementVector = pos - oldPos;
        //adjust distances
        if (aroundUp)
            angleDistAroundUp += Vector3.SignedAngle(oldToParent, toParent, realUp)*angleDistMulti;
        
    }

    protected void Update()
    {
        if ((parent.activeWatch == null && parent.activeBone == null)||angleDistAroundUp==0)
            return;

        if (parent.activeWatch != null)
        {
            parent.activeWatch.ChangeAngle(angleDistAroundUp * stopWatchRotModifier);
            if (angleDistAroundUp < 0)
                angleDistAroundUp = 0;
            return;
        }


        //calculate angular velocities around axies
        Vector3 aVelocity = parent.activeBone.Rb.angularVelocity;

        //magnitude of the projection onto a normal is the dot product
        float velocityAroundUp = Vector3.Dot(aVelocity, realUp);
     
        //adjust distances
        float time = Time.deltaTime;

        //calculate velocity corrections
        velocityAroundUp = Mathf.Sign(angleDistAroundUp) * Mathf.Sqrt(2 * Mathf.Abs(angleDistAroundUp) * acceleration) * Mathf.Deg2Rad - velocityAroundUp;

        //apply corrections
        aVelocity += velocityAroundUp * realUp*parent.activeBone.FlippedMultiplier;


        //remove any rotation along toParent, it is unwanted
        //aVelocity -= Vector3.Dot(aVelocity, toParent) * toParentPerp;

        //push calculated value to the rigidbody if not changing between negative and positive angles
        float angleChange = time * velocityAroundUp * Mathf.Rad2Deg;
        if ((angleDistAroundUp > 0 && angleDistAroundUp -angleChange <0) ||(angleDistAroundUp <0 && angleDistAroundUp + angleChange > 0))
        {
            Debug.Log(angleDistAroundUp);
            aVelocity = Vector3.zero;
            angleDistAroundUp = 0;
            angleChange = 0;
            
        }
        parent.activeBone.Rb.angularVelocity = aVelocity;
        angleDistAroundUp -= time * velocityAroundUp * Mathf.Rad2Deg;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (parent && parent.isActiveAndEnabled)
            parent.StopRotation(acceleration * Mathf.Deg2Rad);
    }
}
