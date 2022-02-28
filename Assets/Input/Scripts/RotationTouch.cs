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
    //private bool aroundUp = true;

    //game feel values
    [SerializeField]
    float aroundToParentMult = 1000;
    [SerializeField]
    float acceleration = 1000f;
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

        //aroundUp = true;
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


        Vector3 movementVector = pos - oldPos;

        //adjust distances
        angleDistAroundUp += Vector3.SignedAngle(oldToParent, toParent, Vector3.up);


        //if (oldToParent != toParent)
            //Debug.Log(oldToParent + ", " + parent);
     
       
    }

    protected void Update()
    {
        

        if (parent.activeWatch == null&& parent.activeBone == null)
            return;
        
            Vector3 aVelocity;
            Vector3 directionality;
            //cheating code for the milestone
             
            GrabbableGroup bone = parent.activeBone;
            Rigidbody boneRB = bone.Rb;
            aVelocity = boneRB.angularVelocity;
        
            if (bone.rightFoward)
            {
                directionality = new Vector3(1.0000000f, 0, 0);

            }
            else
            {
                directionality = new Vector3(0, 0, 1.00000f);
            }
         //up will be the cross product to our bone's forward(Main camera's forward *-1) vector and a Horizontal plane?
            //newUp = Vector3.Cross(new Vector3(1.0f, 0, 0), Camera.main.transform.forward * -1);


            //calculate angular velocities around axies


            //magnitude of the projection onto a normal is the dot product
            float velocityAroundUp = Vector3.Dot(aVelocity, Vector3.up);
           

            //adjust distances
            float time = Time.deltaTime;
            
            angleDistAroundUp -= time * velocityAroundUp * Mathf.Rad2Deg;

            //calculate velocity corrections
            velocityAroundUp = Mathf.Sign(angleDistAroundUp) * Mathf.Sqrt(2 * Mathf.Abs(angleDistAroundUp) * acceleration) * Mathf.Deg2Rad - velocityAroundUp;
            

            //apply corrections
            aVelocity += velocityAroundUp * Vector3.up;
           

            //remove any rotation along toParent, it is unwanted
            aVelocity -= Vector3.Dot(aVelocity, toParent) * toParentPerp;

            //push calculated value to the rigidbody
            Debug.Log(aVelocity);
            parent.activeBone.Rb.angularVelocity = new Vector3(aVelocity.x*directionality.x, aVelocity.y*directionality.y, aVelocity.z*directionality.z);


        //Stopwatch code here

    }







    protected override void OnDisable()
    {
        base.OnDisable();
        if (parent && parent.isActiveAndEnabled)
            parent.StopRotation(acceleration * Mathf.Deg2Rad);
    }

}
