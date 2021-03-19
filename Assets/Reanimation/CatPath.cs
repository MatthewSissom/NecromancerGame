using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatPath
{
    //---Path component classes---//
    private abstract class PathComponent
    {
        public float Duration { get; protected set; }
        public abstract Vector3 GetPointNearPath(float time, float distanceFromPath, bool rightOfPath);
        public abstract Vector3 GetPointOnPath(float time);
        public abstract Vector3 GetPointOnPath(float time, out Vector3 forward);
    }
    private class LinePath : PathComponent
    {
        Vector3 start;
        Vector3 end;
        Vector3 forward;

        public LinePath(float duration, Vector3 start, Vector3 end)
        {
            Duration = duration;
            this.start = start;
            this.end = end;
            forward = (end - start);
            forward.y = 0;
            forward.Normalize();
        }

        public override Vector3 GetPointOnPath(float time, out Vector3 forward)
        {
            forward = this.forward;
            return Vector3.Lerp(start, end, time / Duration);
        }

        public override Vector3 GetPointOnPath(float time)
        {
            return Vector3.Lerp(start, end, time / Duration);
        }

        public override Vector3 GetPointNearPath(float time, float distanceFromPath, bool rightOfPath)
        {
            Vector3 inital = Vector3.Lerp(start, end, time / Duration);
            //get a perpendicular normal vector and scale it by distance from path
            Vector3 offset = new Vector3(-forward.z,0,forward.x) 
                * distanceFromPath
                * (rightOfPath? -1 : 1);

            return inital + offset;
        }
    }
    private class SemicirclePath : PathComponent
    {
        Vector3 center;
        float rad;
        float startTheta;
        float deltaTheta;

        public SemicirclePath(float duration, Vector3 center, float rad, float startTheta, float endTheta)
        {
            Duration = duration;
            this.center = center;
            this.rad = rad;
            this.startTheta = startTheta;
            deltaTheta = endTheta - startTheta;
        }

        public override Vector3 GetPointOnPath(float time)
        {
            float theta = startTheta + deltaTheta * (time / Duration);
            return center + new Vector3(Mathf.Cos(theta) * rad, 0, Mathf.Sin(theta) * rad);
        }
        public override Vector3 GetPointOnPath(float time, out Vector3 forward)
        {
            float theta = startTheta + deltaTheta * (time / Duration);
            Vector3 fromCenterToPath = new Vector3(Mathf.Cos(theta) * rad, 0, Mathf.Sin(theta) * rad);
            forward = new Vector3(-fromCenterToPath.z, 0, fromCenterToPath.x) * Mathf.Sign(deltaTheta);
            return center + fromCenterToPath;
        }

        public override Vector3 GetPointNearPath(float time, float distanceFromPath, bool rightOfPath)
        {
            float theta = startTheta + deltaTheta * (time / Duration);
            //either grow or shrink the rad depending on the orientation of the path (clockwise or not)
            //and if the point should be on the left or right side of it. XOR is used because it is the 
            //only logical opperator that the output flips if an input flips
            float newRad = rad + distanceFromPath * ((rightOfPath ^ deltaTheta < 0) ? 1 : -1);
            return center + new Vector3(Mathf.Cos(theta) * newRad, 0, Mathf.Sin(theta) * newRad);
        }
    }

    //---pathfinding settings---//

    //the tightest turn this cat can make
    public float MinTurningRad { get; set; }
    //how fast the cat should move
    public float Speed { get; set; }
    public float HipDelay { get; set; }

    //---path data---//
    //information about the path the cat is currently taking

    //holds all the components in the current path
    LinkedList<PathComponent> path;
    //the total time required to finish the path
    float totalDuration;
    //how much time has elapsed since the path was first started
    float elapsedTime;
    //how far behind the hips are on the path compared to the sholders

    public CatPath()
    {
        //default values for pathfinding settings
        MinTurningRad = .1f;
        Speed = .1f;
    }

    public void PathToPoint(Vector3 destination, Vector3 currentPos, Vector3 currentHipPos, Vector3 currentForward)
    {
        ResetPath(currentPos,currentHipPos);
        OrientAndMoveTo(destination, currentPos, currentForward);
        FinalizePath();
    }

    public void PathToPoints(List<Vector3> destinations, Vector3 currentPos, Vector3 currentHipPos, Vector3 currentForward)
    {
        ResetPath(currentPos,currentHipPos);
        Vector3 startingPos = currentPos;
        foreach(var destination in destinations)
        {
            //change the current forward to the forward the cat will have at the
            //end of the previous section
            currentForward = OrientAndMoveTo(destination, startingPos, currentForward);
            //similarlly set the starting position to the destination of the last section
            startingPos = destination;
        }
        FinalizePath();
    }

    public Vector3 PointNearPath(float timeFromShoulders, float distanceFromPath, bool rightOfPath, bool includeHipOffset = false)
    {
        timeFromShoulders += elapsedTime - (includeHipOffset ? HipDelay : 0);
        var node = path.First;
        while (timeFromShoulders > node.Value.Duration)
        {
            timeFromShoulders -= node.Value.Duration;
            if (node.Next == null)
            {
                timeFromShoulders = node.Value.Duration;
                break;
            }
            node = node.Next;
        }
        return node.Value.GetPointNearPath(timeFromShoulders, distanceFromPath, rightOfPath);
    }

    //updates the main position on the path, returns false when the path end has been reached
    public bool Move(float deltaTime, out Vector3 shoulderPosition, out Vector3 hipPosition, out Vector3 forward)
    {
        elapsedTime += deltaTime;
        if (elapsedTime > totalDuration)
            elapsedTime = totalDuration;

        LinkedListNode<PathComponent> node = path.First;
        LinkedListNode<PathComponent> hipNode = null;
        float currentDiff = 0;
        while (elapsedTime > node.Value.Duration + currentDiff)
        {
            //hips haved moved past the first section, it isn't needed
            if (elapsedTime - HipDelay > node.Value.Duration)
            {
                elapsedTime -= node.Value.Duration;
                totalDuration -= node.Value.Duration;
                path.RemoveFirst();
                node = path.First;
            }
            else
            {
                if (hipNode == null)
                    hipNode = node;
                currentDiff += node.Value.Duration;
                node = node.Next;
                if (node == null)
                {
                    shoulderPosition = new Vector3();
                    hipPosition = new Vector3();
                    forward = new Vector3();
                    return false;
                }
            }
        }

        shoulderPosition = node.Value.GetPointOnPath(elapsedTime - currentDiff, out forward);
        if(hipNode != null)
            hipPosition = hipNode.Value.GetPointOnPath(elapsedTime - HipDelay);
        else
            hipPosition = node.Value.GetPointOnPath(elapsedTime - HipDelay);

        return elapsedTime < totalDuration;
    }

    //called before creating a new path
    private void ResetPath(Vector3 shoulderPos, Vector3 hipPos)
    {
        //start every path with a path from the hips to the shoulders, and 
        //set elapsedTime to hipdelay
        path = new LinkedList<PathComponent>();
        path.AddFirst(new LinePath(HipDelay,
            hipPos,
            shoulderPos
            ));
        elapsedTime = HipDelay;
    }

    //called to finalize a path
    private void FinalizePath()
    {
        totalDuration = 0;
        foreach (var pathComponent in path)
            totalDuration += pathComponent.Duration;

        if (path.Count == 0)
            Debug.LogError("No path components added to path");
    }

    private Vector3 OrientAndMoveTo(Vector3 destination, Vector3 currentPos, Vector3 currentForward)
    {
        //add a curve that ends with the cat pointing at the destination
        OrientTwardsPoint(destination, currentPos, currentForward, out Vector3? lineStart);

        //change current pos to lineStart if orientation added a path
        if (lineStart != null)
            currentPos = lineStart.Value;

        //move to the destination
        Vector3 toDest = destination - currentPos;
        path.AddLast(new LinePath(
            toDest.magnitude / Speed,
            currentPos,
            destination
            ));

        return toDest.normalized;
    }

    //adds a semicircular path section that orients a cat to face twards their final destination
    private void OrientTwardsPoint(Vector3 destination, Vector3 currentPos, Vector3 currentForward, out Vector3? orientedPoint)
    {
        orientedPoint = null;

        //get delta vector to the destination
        Vector3 currentToDest = destination - currentPos;

        //check to see if the point is already oriented
        if (Mathf.Abs(currentForward.x / currentToDest.x - currentForward.z / currentToDest.z) < 0.01f
            && Mathf.Sign(currentForward.x) == Mathf.Sign(currentToDest.x))
            return;

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
            return;

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
            endTheta += Mathf.PI * -2 *rotationDirection;
        }

        //add the calculated semicircle to the path
        path.AddLast(
            new SemicirclePath(
                Mathf.Abs(endTheta-startTheta) * MinTurningRad / Speed,
                center,
                MinTurningRad,
                startTheta,
                endTheta
                ));
    }
}
