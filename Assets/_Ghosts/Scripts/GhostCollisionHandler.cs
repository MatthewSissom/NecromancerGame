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

        if (otherGo.CompareTag("Ghost"))
        {
            //???
        }

        if(shockRating == 2)
        {
            collided.MajorShock(false);
        }
        else if(shockRating == 1)
        {
            if (!visable)
                collided.MajorShock(false);
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

    /// <summary>
    /// Calculates if a Collision would be noticable, value of 1, or major, value of 2. Otherwise returns 0
    /// Should be moved to ghost behavior
    /// </summary>
    /// <param name="collision">Collision object to check the severity of</param>
    /// <returns></returns>
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
