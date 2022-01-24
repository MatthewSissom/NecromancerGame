using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// unfinished, should have the same structure as bone collision handler, ghosts' OnCollision events should call addCollision
public class GhostCollisionHandler
{
    public void AddCollision(GhostBehavior collided, Collision collision)
    {
        var otherGo = collision.gameObject;


        bool visable = IsVisable(collided, otherGo.transform.position) || IsVisable(collided, collision.contacts[0].point);
        int shockRating = ImpactSeverity(collision);

        //ghost on ghost ignores 
        if (otherGo.CompareTag("Ghost"))
        {

        }
        if(shockRating == 2)
        {
            collided.MajorShock();
        }
        else if(shockRating == 1)
        {
            if (!visable)
                collided.MajorShock();
            else
                collided.MinorShock();
        }
    }

    //returns a val based on if the ghost would have seen the colliding object coming
    bool IsVisable(GhostBehavior ghost, Vector3 collisionPos)
    {
        Vector3 toCollision = (collisionPos - ghost.transform.position).normalized;
        return Vector3.Angle(toCollision,ghost.transform.forward) < 45;
    }

    int ImpactSeverity(Collision collision)
    {
        const float noticableImpactThreshold = .01f;
        const float majorImpactThreshold = .05f;
        if(collision.impulse.magnitude > majorImpactThreshold)
            return 2;
        if (collision.impulse.magnitude > noticableImpactThreshold)
            return 1;
        return 0;
    }
}
