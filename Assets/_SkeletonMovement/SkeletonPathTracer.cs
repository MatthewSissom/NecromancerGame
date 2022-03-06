using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonPathTracer : MonoBehaviour
{
    [SerializeField]
    public Transform Target { private get; set; }
    public Vector3 Position { get { return Target.position; } }
    public float Completion {
        get
        {
            if (path == null)
                return 1;
            return elapsedTime / path.Duration;
        } 
    }

    private float elapsedTime;
    private ISkeletonPath path;

    private void Update()
    {
        // Nothing to trace, return
        if (path == null)
            return;


    }

    private void RemoveFirstComponent()
    {
        var toRemove = path.First.Value;
        elapsedTime -= toRemove.Duration;
        totalDuration -= toRemove.Duration;
        path.RemoveFirst();
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
                if (index == shoulderIndex)
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
}
