using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SkeletonPathfinding
{
    //---pathfinding settings---//

    //the tightest turn this cat can make
    public float MinTurningRad { get; set; }
    //how fast the cat should move
    public float Speed { get; set; }

    public float GroundHeight { get; set; }
    float catChestHeight;

    public SkeletonPathfinding(float catChestHeight, float[] delays, Transform[] transforms, int shoulderIndex)
    {
        //default values for pathfinding settings
        MinTurningRad = .1f;
        Speed = .1f;
        this.catChestHeight = catChestHeight;
        ChestHeightChange += (float val) => { GroundHeight = val-catChestHeight; };
    }

    public bool PathToPoint(Vector3 destination)
    {
        if (FollowingSplit)
        {
            queuedDestination = destination;
            return true;
        }

        Vector3 pathStart = shoulderTransform.position;
        float currentGround = pathStart.y =  /*destination.y =*/  GroundHeight;

        //get a series of points from unity's navmesh
        NavMeshPath mPath = new NavMeshPath();
        if (!NavMesh.CalculatePath(pathStart, destination, NavMesh.AllAreas, mPath))
        {
            // no path, return
            return false;
        }

#if UNITY_EDITOR
        DebugRendering.UpdatePath(DebugModes.DebugPathFlags.NavMeshPath, mPath.corners);
#endif

        var points = new List<Vector3>();
        Vector3 previous = shoulderTransform.position;
        for (int i = 1, length = mPath.corners.Length-1; i < length; i++)
        {
            Vector3 vector = mPath.corners[i];
            Vector3 toPrevious = previous - vector;
            float distance = toPrevious.magnitude;

            float jumpHeight = -toPrevious.y;
            if (Mathf.Abs(jumpHeight) > .05f)
            {
                currentGround += jumpHeight + catChestHeight;
                vector.y = currentGround + catChestHeight;
                previous = vector;
                points.Add(vector);
                continue;
            }

            vector.y = currentGround + catChestHeight;

            //slightly shorten targets to make curving around corners feel more natural
            float radMult = -Mathf.Tan(
                Mathf.Deg2Rad * Vector3.Angle(
                    toPrevious, (mPath.corners[i + 1] - vector)
                    )
                );
            radMult = Mathf.Min(1, radMult);
            vector += toPrevious / distance * radMult* MinTurningRad;

            //skip points that would cause orientandpathto to fail
            if ((previous - vector).magnitude < MinTurningRad)
                continue;

            previous = vector;

            points.Add(vector);
        }

        Vector3 lastVector = mPath.corners[mPath.corners.Length-1];
        lastVector.y = currentGround + catChestHeight;
        points.Add(lastVector);

#if UNITY_EDITOR
        DebugRendering.UpdatePath(DebugModes.DebugPathFlags.ModifiedNavMeshPath, points);
#endif

        return PathToPoints(points);
    }

    public virtual bool PathToPoint(Vector3 destination)
    {
        if (FollowingSplit)
        {
            queuedDestination = destination;
            return true;
        }

        ResetPath();

        if (OrientAndPathTo(destination, shoulderTransform.position, shoulderTransform.forward, out Vector3 forward))
            FinalizePath();
        else
            return false;

        return true;
    }

    public bool PathToPoints(List<Vector3> destinations)
    {
        ResetPath();
        Vector3 pos = shoulderTransform.position;
        Vector3 forward = shoulderTransform.forward;
        foreach (var destination in destinations)
        {
            //height changes require a jump
            if (Mathf.Abs(destination.y - pos.y) > .07f)
            {
                JumpToPoint(destination, pos, .1f);
            }
            else
            {
                //change the current forward to the forward the cat will have at the
                //end of the previous section
                if (!OrientAndPathTo(destination, pos, forward, out forward))
                {
                    Debug.Log("bad path aborting");
                    return false;
                }
            }

            //update the position for next loop
            pos = destination;
        }
        FinalizePath();
        return true;
    }

    private bool OrientAndPathTo(Vector3 destination, Vector3 currentPos, Vector3 currentForward, out Vector3 endForward)
    {
        //add a curve that ends with the cat pointing at the destination
        OrientTwardsPoint(destination, currentPos, currentForward, out Vector3? lineStart);

        //change current pos to lineStart if orientation added a path
        if (lineStart != null)
        {
            currentPos = lineStart.Value;
        }

        //move to the destination
        Vector3 toDest = destination - currentPos;
        path.AddLast(new LinePath(
            toDest.magnitude / Speed,
            currentPos,
            destination
            ));

        endForward = toDest.normalized;
        return true;
    }

    //adds a semicircular path section that orients a cat to face twards their final destination
    //returns true if it was sucessful in orienting
    private bool OrientTwardsPoint(Vector3 destination, Vector3 currentPos, Vector3 currentForward, out Vector3? orientedPoint)
    {
        orientedPoint = null;

        //get delta vector to the destination
        Vector3 currentToDest = destination - currentPos;

        //check to see if the point is already oriented
        if (Mathf.Abs(currentForward.x / currentToDest.x - currentForward.z / currentToDest.z) < 0.01f
            && Mathf.Sign(currentForward.x) == Mathf.Sign(currentToDest.x))
            return true;

        //center of the circle should be the perpendicular to the forwardVector and as close to
        //the destination as possible
        Vector3 currentToCircleCenter = new Vector3(-currentForward.z, 0, currentForward.x);  //get perpendicular of forward
        currentToCircleCenter = Vector3.Project(currentToDest, currentToCircleCenter);        //project the delta on to the perpendicular
        currentToCircleCenter = currentToCircleCenter.normalized * MinTurningRad;             //adjust length to equal rad

        Vector3 center = currentPos + currentToCircleCenter;
        Vector3 delta = new Vector3(destination.x - center.x, 0, destination.z - center.z);
        Vector3 deltaPerp = new Vector3(-delta.z, 0, delta.x);
        float deltaMagnitude = delta.magnitude;
        if (deltaMagnitude < MinTurningRad) //algorithm doesn't work for points inside the circle
        {
            return false;
        }

        //calculate points of tangency on the circle passing through the destination point by constructing
        //a similar triangle with r being similar to the tangent line passing through p
        float scaleFactor = MinTurningRad / deltaMagnitude; //divide hypotonuses to get scale factor
        float aScale = scaleFactor * scaleFactor;
        float bScale = scaleFactor * Mathf.Sqrt(1 - aScale);

        //calculate vector from the center of the circle to a point of tangency
        Vector3 pointOfTangency = new Vector3(aScale * delta.x + bScale * deltaPerp.x,
            0,
            aScale * delta.z + bScale * deltaPerp.z);

        //check to see if the point of tangency has the same orientation (clockwise or counter) as the forward vector
        //by seeing if the radious is the left or right perpendicular of the vector
        //a vector perp of v, called u = c*(-v2,v1) the direction of u (left of v or right of v) can be found by 
        //can be found by checking the sign of v1 * u2. if positive c > 0 and u is to the left of v
        float forwardDirection = Mathf.Sign(currentForward.x * currentToCircleCenter.z);
        if (Mathf.Sign((delta.x - pointOfTangency.x) * -pointOfTangency.z) != forwardDirection)
        {
            pointOfTangency = new Vector3(aScale * delta.x - bScale * deltaPerp.x,
                 0,
                 aScale * delta.z - bScale * deltaPerp.z);
        }

        //set the world space oriented point
        Vector3 temp = pointOfTangency + center;
        temp.y = destination.y;
        orientedPoint = temp;
        center.y = destination.y;

        //calculate thetas for semicircle path
        float startTheta = Mathf.Atan2(-currentToCircleCenter.z, -currentToCircleCenter.x);
        float endTheta = Mathf.Atan2(pointOfTangency.z, pointOfTangency.x);

        float rotationDirection = Mathf.Sign(endTheta - startTheta);
        if (rotationDirection != forwardDirection)
        {
            endTheta += Mathf.PI * -2 * rotationDirection;
        }

        //add the calculated semicircle to the path
        path.AddLast(
            new SemicirclePath(
                Mathf.Abs(endTheta - startTheta) * MinTurningRad / Speed,
                center,
                MinTurningRad,
                startTheta,
                endTheta
                ));

        return true;
    }
}
