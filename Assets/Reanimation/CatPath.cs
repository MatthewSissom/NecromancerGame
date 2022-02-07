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
    protected int shoulderIndex;
    protected Transform shoulderTransform;

    //---path data---//
    //information about the path the cat is currently taking

    public bool IsValid { get { return path != null && path.Count != 0; } }
    public bool FollowingSplit { get; private set; }
    //holds all the components in the current path
    LinkedList<PathComponent> path;
    //the total time required to finish the path
    float totalDuration;
    //how much time has elapsed since the path was first started
    float elapsedTime;

    //---events---//
    public event System.Action PathFinished;
    public event System.Action PathStarted;
    public event System.Action PathReset;
    public event System.Action<float> ChestHeightChange;
    public event System.Action<Jump> JumpStarted;

    public CatPath(float[] delays, Transform[] transforms, int shoulderIndex)
    {
        //default values for pathfinding settings
        MinTurningRad = .1f;
        Speed = .1f;
        this.delays = delays;

        int hipIndex = System.Math.Max(shoulderIndex - 1, 0); 
        hipDelay = delays[hipIndex];

        this.transforms = transforms;
        this.shoulderIndex = shoulderIndex;
        shoulderTransform = transforms[shoulderIndex];
    }

    public virtual bool PathToPoint(Vector3 destination)
    {
        if(FollowingSplit)
        {
            void QueueNewPath()
            {
                PathToPoint(destination);
                PathReset -= QueueNewPath;
            }
            PathReset += QueueNewPath;
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
        if (FollowingSplit)
        {
            return false;
        }

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
        if (FollowingSplit)
            TraceSplitPath(deltaTime, out forward, newPositions);
        else
            TraceLine(deltaTime, out forward, newPositions);
    }

    public void TraceSplitPath(float deltaTime, out Vector3 forward, Vector3[] newPositions)
    {
        forward = default;
        elapsedTime += deltaTime;
        var splitPath = path.First.Value as SplitPath;

        //check if the path has ended
        if(elapsedTime > totalDuration)
        {
            //invoke height change if jumping
            if (splitPath is Jump)
            {
                splitPath.SetIndex(shoulderIndex);
                ChestHeightChange?.Invoke(splitPath.GetPointOnPath(splitPath.SplitDuration).y);
            }

            //check to see if this was the last path
            if (path.Count == 1)
            {
                elapsedTime = totalDuration;
                PathFinished?.Invoke();
                forward = shoulderTransform.forward;
            }
            //if there is another split path trace it
            else if(path.First.Next != null && path.First.Next.Value.IsSplit)
            {
                //remove this split path and trace the next one
                RemoveFirstComponent();
                FollowingSplitInit();
            }
            //otherwise switch back to following standard paths
            else
            {
                 ToggleFollowingSplit();
            }
            return;
        }

        for(int i = 0, count = transforms.Length; i < count; i++)
        {
            var t = transforms[i];
            splitPath.SetIndex(i);

            if (t == shoulderTransform)
                newPositions[i] = splitPath.GetPointOnPath(elapsedTime, out forward);
            else
                newPositions[i] = splitPath.GetPointOnPath(elapsedTime);
        }
    }

    public void TraceLine(float deltaTime, out Vector3 forward, Vector3[] newPositions)
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
                if(index == shoulderIndex)
                {
                    if (node.Value.IsSplit)
                        ToggleFollowingSplit();
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
        //if the cat's target was changed while following another path use the old
        //path for higher precision
        else if(path.First.Value as SplitPath == null)
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
        else
        {
            var split = path.First.Value as SplitPath;
            Vector3 pos;
            for (int i = transforms.Length - 2; i >= 0; i--)
            {
                split.SetIndex(i);
                pos = split.GetPointOnPath(split.SplitDuration);
                if (delays[i] > 0)
                {
                    newPath.AddFirst(new LinePath(delays[i] - delays[i + 1],
                        pos,
                        previousPos
                        ));
                }
                previousPos = pos;
            }
            elapsedTime = delays[0];
        }

        //start every path with linar paths for transforms behind the shoulders to follow
        path = newPath;

        PathReset?.Invoke();
    }

    private void ToggleFollowingSplit()
    {
        if (FollowingSplit)
            SoftReset();
        else
            FollowingSplitInit();

        FollowingSplit = !FollowingSplit;
    }

    private void FollowingSplitInit()
    {

        //remove all non-split paths before the first split
        LinkedListNode<PathComponent> newStart = path.First;
        while (!newStart.Value.IsSplit)
        {
            path.RemoveFirst();
            newStart = path.First;
            //if there are no more nodes the path is finished
            if (newStart == null)
            {
                PathFinished?.Invoke();
                return;
            }
        }

        var jump = newStart.Value as Jump;
        if (jump != null)
            JumpStarted?.Invoke(jump);


        totalDuration = (path.First.Value as SplitPath).SplitDuration;
        elapsedTime = 0;
    }

    //called after any split path to ensure that all transforms will be aligned to the standard path
    private void SoftReset()
    {
        //reset path needs in tack current path
        var oldPath = path;
        ResetPath();

        //add old path back to the new path, except for 
        //the traced split component and the padding
        oldPath.RemoveFirst();
        var padding = oldPath.Last.Value;
        foreach (var pc in oldPath)
            if(pc != padding)
                path.AddLast(pc);
        FinalizePath();
    }

    private void RemoveFirstComponent()
    {
        var toRemove = path.First.Value;
        elapsedTime -= toRemove.Duration;
        totalDuration -= toRemove.Duration;
        path.RemoveFirst();
    }

    private void FinalizePath()
    {
        totalDuration = 0;

        foreach (var pathComponent in path)
        {
            totalDuration += pathComponent.Duration;
            if (pathComponent.IsSplit)
                (pathComponent as SplitPath).UseStallingPath();
        }

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

#if UNITY_EDITOR
        // sample points on path for debugging
        var node = path.First;
        PathComponent currentPathComponent = node.Value;
        List<Vector3> points = new List<Vector3>();
        float simulatedTime = 0;
        const float simulatedTimeStep = .3f;
        while (node != null)
        {
            if (currentPathComponent.Duration > simulatedTime)
            {
                points.Add(currentPathComponent.GetPointOnPath(simulatedTime));
                simulatedTime += simulatedTimeStep;
            }
            else
            {
                node = node.Next;
                simulatedTime -= currentPathComponent.Duration;
                currentPathComponent = node?.Value;
            }
        }
        DebugRendering.UpdatePath(DebugModes.SkeletonPathFlags.TruePath, points);
#endif

        PathStarted?.Invoke();
    }

    private bool JumpToPoint(Vector3 destination, Vector3 currentPos, float aditionalApexHeight)
    {
        var delta = destination - currentPos;
        Vector3[] starts = new Vector3[transforms.Length];
        Vector3[] destinations = new Vector3[transforms.Length];

        //get info about the path so far so starts can be created
        float currentDuration = 0;
        foreach (var pc in path)
            currentDuration += pc.Duration;

        //create stalling path
        //needs to be done before starts are calcualted, because paths with negitive delay need stalling path
        var last = path.Last.Value;
        Vector3 start = last.GetPointOnPath(last.Duration, out Vector3 forward);
        var stallingPath = SplitPath.GetDefaultStallingPath(forward,start,Speed);

        //create starts, use existing paths to determine where all transforms will be
        //when the jump starts
        int index = 0;
        var node = path.First;
        PathComponent currentPath = node.Value;
        float total = 0;
        while (index < starts.Length)
        {
            float timeInNode = currentDuration - delays[index] - total;
            if (currentPath.Duration < timeInNode)
            {
                total += node.Value.Duration;
                node = node.Next;
                if (node != null)
                    currentPath = node.Value;
                else if (currentPath != stallingPath)
                    currentPath = stallingPath;
                continue;
            }

            if (currentPath.IsSplit)
            {
                var splitNode = currentPath as SplitPath;
                if (delays[index] >= 0)
                {
                    splitNode.SetIndex(index);
                    starts[index] = splitNode.GetPointOnPath(timeInNode);
                }
            }
            else
            {
                starts[index] = currentPath.GetPointOnPath(timeInNode);
            }
            index++;
        }

        for (int i = 0; i < transforms.Length; i++)
        {
            destinations[i] = starts[i] + delta;
        }


        //adjust apex height
        if (destination.y > currentPos.y)
            aditionalApexHeight += destination.y - currentPos.y;

        //add path
        path.AddLast(new Jump(starts, destinations, aditionalApexHeight,stallingPath));
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
