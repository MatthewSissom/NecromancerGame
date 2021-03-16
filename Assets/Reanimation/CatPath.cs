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
    }
    private class LinePath : PathComponent
    {
        Vector3 start;
        Vector3 end;

        public LinePath(float duration, Vector3 start, Vector3 end)
        {
            Duration = duration;
        }

        public override Vector3 GetPointOnPath(float time)
        {
            return Vector3.Lerp(start, end, time / Duration);
        }

        public override Vector3 GetPointNearPath(float time, float distanceFromPath, bool rightOfPath)
        {
            Vector3 inital = GetPointOnPath(time);
            //get a perpendicular normal vector and scale it by distance from path
            Vector3 offset = Vector3.Cross(end - start, Vector3.up).normalized 
                * distanceFromPath
                * (rightOfPath? 1 : -1);

            return inital + offset;
        }
    }
    private class SemicirclePath : PathComponent
    {
        Vector3 center;
        float rad;
        float height;
        float startTheta;
        float deltaTheta;

        public SemicirclePath(float duration, Vector3 center, float rad, float height, float startTheta, float endTheta)
        {
            Duration = duration;
            this.center = center;
            this.rad = rad;
            this.height = height;
            this.startTheta = startTheta;
            deltaTheta = endTheta - startTheta;
        }

        public override Vector3 GetPointOnPath(float time)
        {
            float theta = startTheta + deltaTheta * (time / Duration);
            return new Vector3(Mathf.Cos(theta) * rad,
                height,
                Mathf.Sin(theta) * rad);
        }

        public override Vector3 GetPointNearPath(float time, float distanceFromPath, bool rightOfPath)
        {
            float theta = startTheta + deltaTheta * (time / Duration);
            //either grow or shrink the rad depending on the orientation of the path (clockwise or not)
            //and if the point should be on the left or right side of it. XOR is used because it is the 
            //only logical opperator that the output flips if an input flips
            float newRad = rad + distanceFromPath * ((rightOfPath ^ deltaTheta < 0) ? 1 : -1); 
            return new Vector3(Mathf.Cos(theta) * newRad,
                height,
                Mathf.Sin(theta) * newRad);
        }
    }

    //---pathfinding settings---//

    //the tightest turn this cat can make
    public float MinTurningRad { get; set; }
    //how fast the cat should move
    public float Speed { get; set; }

    //---path data---//
    //information about the path the cat is currently taking

    //holds all the components in the current path
    LinkedList<PathComponent> path;
    //the total time required to finish the path
    float totalDuration;
    //how much time has elapsed since the path was first started
    float elapsedTime;
    //how far behind the hips are on the path compared to the sholders
    float hipDelay;

    public CatPath()
    {
        //default values for pathfinding settings
        MinTurningRad = .1f;
        Speed = .1f;
    }

    public void PathToPoint(Vector3 destination, Vector3 currentPos, Vector3 currentForward)
    {
        ResetPath();
        OrientAndMoveTo(destination, currentPos, currentForward);
        FinalizePath();
    }

    public void PathToPoints(List<Vector3> destinations, Vector3 currentPos, Vector3 currentForward)
    {
        ResetPath();
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

    //updates the main position on the path, returns false when the path end has been reached
    bool Move(float deltaTime, out Vector3 shoulderPosition, out Vector3 hipPosition)
    {
        elapsedTime += deltaTime;
        if (elapsedTime > totalDuration)
            elapsedTime = totalDuration;

        LinkedListNode<PathComponent> node = path.First;
        LinkedListNode<PathComponent> hipNode = null;
        float currentDiff = 0;
        while (elapsedTime > node.Value.Duration)
        {
            //hips haved moved past the first section, it isn't needed
            if (elapsedTime - hipDelay > node.Value.Duration)
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
            }
        }

        shoulderPosition = node.Value.GetPointOnPath(elapsedTime - currentDiff);
        if(hipNode != null)
            hipPosition = hipNode.Value.GetPointOnPath(elapsedTime - hipDelay);
        else
            hipPosition = node.Value.GetPointOnPath(elapsedTime - hipDelay);
        return elapsedTime == totalDuration;
    }

    //called before creating a new path
    private void ResetPath()
    {
        path = new LinkedList<PathComponent>();
        totalDuration = 0;
        elapsedTime = 0;
    }

    private void FinalizePath()
    {
        elapsedTime = 0;
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

        Vector3 currentToDest = destination - currentPos;
        //use dot product to see if the forward is to the left or right of the toDest vector, which determines direction of rotation
        bool isCounterClockwise = Vector3.Dot(new Vector3(-currentToDest.z, 0, currentToDest.x), currentForward) > 0;

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

        //check to see if the vector is on the same side of delta, as the current forward
        //if not, use the other tangent vector
        if(Vector3.Dot(pointOfTangency,deltaPerp) > 0 != isCounterClockwise )
            pointOfTangency = new Vector3(aScale * delta.x - bScale * deltaPerp.x,
                 0,
                 aScale * delta.z + bScale * deltaPerp.z);

        //set the world space oriented point
        orientedPoint = pointOfTangency + center + new Vector3(0,destination.y,0);

        //calculate thetas for semicircle path
        float startTheta = Mathf.Atan2(-currentToCircleCenter.z, -currentToCircleCenter.x);
        float endTheta = Mathf.Atan2(pointOfTangency.z, pointOfTangency.x);

        //add the calculated semicircle to the path
        path.AddLast(
            new SemicirclePath(
                Mathf.Abs(endTheta-startTheta) * MinTurningRad,
                center,
                destination.y,
                MinTurningRad,
                startTheta,
                endTheta
                ));
    }
}
