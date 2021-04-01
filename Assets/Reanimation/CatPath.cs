using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CatPath
{

    //---pathfinding settings---//

    //the tightest turn this cat can make
    public float MinTurningRad { get; set; }
    //how fast the cat should move
    public float Speed { get; set; }
    //public float HipDelay { get; set; }

    //---transform arrays---//
    //arrays that hold information about key points along the cat's main line. data is ordered
    //starting at the BACK of the cat moving forward

    //the different time differences from the shoulder of each transform along the cat's main line
    //a delay of 1 would mean one second after the shoulders pass a point, the delayed transform
    //would pass the same point on the path
    float[] delays;
    //the delay of the hips relitive to the shoulders
    float hipDelay;
    protected Transform[] transforms;
    protected Transform shoulderTransform;

    //---path data---//
    //information about the path the cat is currently taking

    public bool IsValid { get { return path != null && path.Count != 0; } }
    //holds all the components in the current path
    LinkedList<PathComponent> path;
    //the total time required to finish the path
    float totalDuration;
    //how much time has elapsed since the path was first started
    float elapsedTime;
    //how far behind the hips are on the path compared to the sholders

    //---events---//
    public event System.Action PathFinished;
    public event System.Action PathStarted;
    public event System.Action PathReset;

    public CatPath(float [] delays, Transform[] transforms, int hipIndex)
    {
        //default values for pathfinding settings
        MinTurningRad = .1f;
        Speed = .1f;
        this.delays = delays;
        hipDelay = delays[hipIndex];

        this.transforms = transforms;
        shoulderTransform = transforms[hipIndex + 1];
    }

    public virtual bool PathToPoint(Vector3 destination)
    {
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
            //change the current forward to the forward the cat will have at the
            //end of the previous section
            if (!OrientAndPathTo(destination, pos, forward, out forward))
            {
                Debug.Log("bad path aborting");
                return false;
            }
            //similarlly set the starting position to the destination of the last section
            pos = destination;
        }
        FinalizePath();
        return true;
    }

    public Vector3 PointNearPath(float timeFromShoulders, float distanceFromPath, bool rightOfPath, bool includeHipOffset = false)
    {
        timeFromShoulders += elapsedTime - (includeHipOffset ? hipDelay : 0);
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

    public bool GetPointOnPath(float timeFromShoulders, out Vector3 point)
    {
        timeFromShoulders += elapsedTime;
        LinkedListNode<PathComponent> node;
        if (path == null
            || (node = path.First) == null)
        {
            point = new Vector3();
            return false;
        }
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
        point = node.Value.GetPointOnPath(timeFromShoulders);
        return true;
    }

    //updates the main position on the path, returns false when the path end has been reached
    public void Move(float deltaTime, out Vector3 forward, Vector3[] newPositions)
    {
        forward = default;     //assignment so forward can't be empty
        elapsedTime += deltaTime;
        if (elapsedTime > totalDuration)
        {
            elapsedTime = totalDuration;
            PathFinished?.Invoke();
            forward = shoulderTransform.forward;
        }

        var node = path.First; //node in the path
        float totalDiff = 0;   //total duration of already processed nodes
        int index = 0;         //index for accessing transform arrays

        bool finished = false;
        while (!finished)
        {
            float timeSpentInPathComponent = elapsedTime - delays[index] - totalDiff;
            //move to the next node if the time is outside the length of 
            if (timeSpentInPathComponent > node.Value.Duration)
            {
                //the last delay has moved past the first section in the path, free memory
                if (index == 0)
                {
                    RemoveFirstComponent();
                    node = path.First;
                }
                else
                {
                    totalDiff += node.Value.Duration;
                    node = node.Next;
                    finished = node == null;
                    if (finished)
                        Debug.Log("wrong");
                }
            }
            else
            {
                //get the forward vector if this transform is the shoulder transform
                if(delays[index] == 0)
                {
                    newPositions[index] = node.Value.GetPointOnPath(elapsedTime - totalDiff, out forward);
                }
                else
                {
                    newPositions[index] = node.Value.GetPointOnPath(timeSpentInPathComponent);
                }
                finished = ++index == delays.Length;
            }
        }
    }

    //called before creating a new path
    private void ResetPath()
    {
        //add paths for each transfrom behind the shoulders to lerp to the position
        //of the key transform infront of it so at every point each key will be on the path
        Vector3 previousPos = default;
        var newPath = new LinkedList<PathComponent>();

        if (path == null)
        {
            //itterate backwards to go from head to tail
            for (int i = transforms.Length - 2; i >= 0; i--)
            {
                float delay = delays[i];
                if (delay > 0)
                {
                    newPath.AddFirst(new LinePath(delay - delays[i + 1],
                        transforms[i].position,
                        previousPos
                        ));
                }
                previousPos = transforms[i].position;
            }
            elapsedTime = delays[0];
        }
        else
        {
            //itterate backwards to go from head to tail
            for (int i = transforms.Length - 2; i >= 0; i--)
            {
                float delay = delays[i];
                if (delay > 0)
                {
                    GetPointOnPath(-delay, out Vector3 mPos);
                    newPath.AddFirst(new LinePath(delay - delays[i + 1],
                        mPos,
                        previousPos
                        ));
                }
                GetPointOnPath(-delay, out previousPos);
            }
            elapsedTime = delays[0];
        }

        //start every path with linar paths for transforms behind the shoulders to follow
        path = newPath;

        PathReset?.Invoke();
    }

    private void RemoveFirstComponent()
    {
        elapsedTime -= path.First.Value.Duration;
        totalDuration -= path.First.Value.Duration;
        path.RemoveFirst();
    }

    private void FinalizePath()
    {
        totalDuration = 0;
        foreach (var pathComponent in path)
            totalDuration += pathComponent.Duration;

        if (path.Count == 0)
            Debug.LogError("No path components added to path");

        Vector3 forward = new Vector3();
        Vector3 previousPos = default;
        for (int i = 0, count = transforms.Length; i < count; i++)
        {
            if (delays[i] > 0)
                continue;

            if (forward == new Vector3())
            {
                previousPos = path.Last.Value.GetPointOnPath(path.Last.Value.Duration, out forward);
            }
            else
            {
                Vector3 pathEnd = forward + previousPos;
                path.AddLast(new LinePath(1 / Speed, previousPos, pathEnd));
                previousPos = path.First.Value.GetPointOnPath(path.Last.Value.Duration);
            }
        }

        PathStarted?.Invoke();
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

        return true;
    }
}
