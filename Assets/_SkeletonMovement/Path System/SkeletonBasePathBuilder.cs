using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SkeletonBasePathBuilder
{
    private SkeletonPathTunables tunables;
    private SkeletonPathData pathData;
    private LinkedList<IContinuousSkeletonPath> path;

    public SkeletonBasePathBuilder(SkeletonPathTunables tunables, SkeletonPathData pathData)
    {
        this.tunables = tunables;
        this.pathData = pathData;
    }

    // should only be called the first time a skeleton starts a path
    public IContinuousSkeletonPath GroundPathFromPoints(Vector3 startForward, Vector3[] destinations)
    {
        Debug.Log("Inital path created");

        path = new LinkedList<IContinuousSkeletonPath>();
        Vector3 start = destinations[0];
        // add line for delayed points
        path.AddFirst(new LinePath(
            pathData.SkeletonDuration,
            start + startForward * -1 * pathData.DelayedPathLenght,
            start
        ));

        // try to path. if failed return null
        if (!CalculateGroundPathInternal(startForward, destinations))
            return null;

        IContinuousSkeletonPath finalPath =  new CompositePath(path, -1 * pathData.SkeletonDuration);
        return finalPath;
    }

    public IContinuousSkeletonPath SwitchGroundPath(IContinuousSkeletonPath previousPath, float traceTime, Vector3[] destinations)
    {
        path = new LinkedList<IContinuousSkeletonPath>();

        // use previous path for delayed points
        float adjustedTraceTime = traceTime - pathData.SkeletonDuration;
        TrimmedPath delayedPath = new TrimmedPath(previousPath, adjustedTraceTime, pathData.SkeletonDuration);
        delayedPath.DeletePathBefore(0);
        path.AddFirst(delayedPath);

        // check that current position and pathfinding start pos are ==
        Assert.AreApproximatelyEqual(previousPath.GetPointOnPath(traceTime).x, destinations[0].x,.01f);
        Assert.AreApproximatelyEqual(previousPath.GetPointOnPath(traceTime).y, destinations[0].y,.01f);
        Assert.AreApproximatelyEqual(previousPath.GetPointOnPath(traceTime).z, destinations[0].z,.01f);

        // try to path. if failed return null
        if (!CalculateGroundPathInternal(previousPath.GetForward(traceTime), destinations))
            return null;

        IContinuousSkeletonPath finalPath = new CompositePath(path, -1 * pathData.SkeletonDuration);
        return finalPath;
    }

    private bool CalculateGroundPathInternal(Vector3 startForward, Vector3[] destinations)
    {
        Vector3 pos = destinations[0];
        Vector3 forward = startForward;
        for(int i = 1; i < destinations.Length; i++)
        {
            Vector3 destination = destinations[i];
            //change the current forward to the forward the cat will have at the
            //end of the previous section
            if (!OrientAndGoToPoint(destination, pos, forward, out forward))
            {
                Debug.Log("bad path aborting");
                return false;
            }

            //update the position for next loop
            pos = destination;
        }

        return true;
    }

    private bool OrientAndGoToPoint(Vector3 destination, Vector3 currentPos, Vector3 currentForward, out Vector3 endForward)
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
            toDest.magnitude / tunables.Speed,
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
        currentToCircleCenter = currentToCircleCenter.normalized * tunables.MinTurningRad;             //adjust length to equal rad

        Vector3 center = currentPos + currentToCircleCenter;
        Vector3 delta = new Vector3(destination.x - center.x, 0, destination.z - center.z);
        Vector3 deltaPerp = new Vector3(-delta.z, 0, delta.x);
        float deltaMagnitude = delta.magnitude;
        if (deltaMagnitude < tunables.MinTurningRad) //algorithm doesn't work for points inside the circle
        {
            return false;
        }

        //calculate points of tangency on the circle passing through the destination point by constructing
        //a similar triangle with r being similar to the tangent line passing through p
        float scaleFactor = tunables.MinTurningRad / deltaMagnitude; //divide hypotonuses to get scale factor
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
                Mathf.Abs(endTheta - startTheta) * tunables.MinTurningRad / tunables.Speed,
                center,
                tunables.MinTurningRad,
                startTheta,
                endTheta
                ));

        return true;
    }
}
