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
    float acceleration = 100;
    [SerializeField]
    float switchResistance = 0;

    //Score - tracks if the player is trying to spin or pinch
    const int scoresLength = 5;
    //holds the scores from the last few frames to avoid switching due to an outlier
    float[] scores = new float[scoresLength];
    int scoreIndex = 0;
    //the sum of everything in scores
    float currentScore;

    //Better up vector
    Vector3 newUp;

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

        aroundUp = true;
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
        
        //update scores and check for axis change//

        //score goes from -.5 to .5 based on if the movement is in the 
        //direction of the parent (.5) or perpendicular to it (-.5)
        Vector3 movementVector = pos - oldPos;
        //split the vector into components
        float twardsParentDistance = Vector3.Dot(movementVector, toParent);     //signed distance
        float perpToParentDistance = Vector3.Dot(movementVector, toParentPerp); //signed distance
        //score is the difference between lengths of component vectors
        float newScore = Mathf.Abs(twardsParentDistance) - Mathf.Abs(perpToParentDistance);

        //adjust score
        currentScore -= scores[scoreIndex]; //remove score that will be replaced from total
        currentScore += newScore;           //add new score to total
        scores[scoreIndex] = newScore;
        scoreIndex = (scoreIndex + 1) % scoresLength;

        //check if the player has switched from spinning to pinching
        if (aroundUp && currentScore > switchResistance)
        {
            aroundUp = false;
        }
        else if (!aroundUp && currentScore < - switchResistance)
        {
            aroundUp = true;
        }

        //adjust distances
        if(aroundUp)
            angleDistAroundUp += Vector3.SignedAngle(oldToParent, toParent, Vector3.up);
        else
            angleDistAroundParent += twardsParentDistance * aroundToParentMult;
       
    }

    protected void Update()
    {

        if (parent.activeWatch == null&& parent.activeBone == null)
            return;

        //cheating code for the milestone
        if(parent.activeBone != null){
            GrabbableGroup bone = parent.activeBone;
            Vector3 directionality;
            Vector3 aVelocity = parent.activeBone.Rb.angularVelocity;
            if (bone.rightFoward)
            {
                directionality = new Vector3(-1.0f, 0, 0);

            }
            else
            {
                directionality = new Vector3(0, 0, -1.0f);
            }

            aVelocity += angleDistAroundUp * directionality;

            bone.Rb.angularVelocity = aVelocity;
        }
        
        //Stopwatch code here


        /* Good and proper code that we will bring back maybe?
        //up will be the cross product to our bone's forward(Main camera's forward *-1) vector and Auxilery axis
        newUp = Vector3.Cross(parent.auxileryAxis, Camera.main.transform.forward * -1);

        

        //calculate angular velocities around axies
        Vector3 aVelocity = parent.activeObj.Rb.angularVelocity;

        //magnitude of the projection onto a normal is the dot product
        float velocityAroundUp = Vector3.Dot(aVelocity, newUp);
        float velocityAroundToParent = Vector3.Dot(aVelocity, toParentPerp);

        //adjust distances
        float time = Time.deltaTime;
        angleDistAroundParent -= time * velocityAroundToParent * Mathf.Rad2Deg;
        angleDistAroundUp -= time * velocityAroundUp * Mathf.Rad2Deg;

        //calculate velocity corrections
        velocityAroundUp = Mathf.Sign(angleDistAroundUp) * Mathf.Sqrt(2 * Mathf.Abs(angleDistAroundUp) * acceleration) * Mathf.Deg2Rad - velocityAroundUp;
        velocityAroundToParent = Mathf.Sign(angleDistAroundParent) * Mathf.Sqrt(2 * Mathf.Abs(angleDistAroundParent) * acceleration) * Mathf.Deg2Rad - velocityAroundToParent;

        //apply corrections
        aVelocity += velocityAroundUp * Vector3.up;
        aVelocity += velocityAroundToParent * toParentPerp;

        //remove any rotation along toParent, it is unwanted
        aVelocity -= Vector3.Dot(aVelocity, toParent) * toParentPerp;

        //push calculated value to the rigidbody
        parent.activeObj.Rb.angularVelocity = aVelocity;
        */

    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (parent && parent.isActiveAndEnabled)
            parent.StopRotation(acceleration * Mathf.Deg2Rad);
    }
}
