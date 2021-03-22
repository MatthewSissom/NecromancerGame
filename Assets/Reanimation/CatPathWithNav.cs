using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatPathWithNav : CatPath
{
    float groundHeight;
    float catChestHeight;
    public CatPathWithNav(float groundHeight, float catChestHeight)
    {
        //default values for pathfinding settings
        MinTurningRad = .1f;
        Speed = .1f;
        this.groundHeight = groundHeight;
        this.catChestHeight = catChestHeight;
    }

    public override void PathToPoint(Vector3 destination, Vector3 currentPos, Vector3 currentHipPos, Vector3 currentForward)
    {
        //if a path exists then choose a point ahead of the cat's current location as the starting position 
        //to avoid the pathfinding having cats make rapid changes in direction
        if(!GetPointOnPath(MinTurningRad / Speed + .25f, out Vector3 pathStart))
        {
            pathStart = currentPos;
        }

        pathStart.y = groundHeight;
        destination.y = groundHeight;
        NavMeshPath mPath = new NavMeshPath();
        if(NavMesh.CalculatePath(pathStart, destination, NavMesh.AllAreas, mPath))
        {
            var points = new List<Vector3>();
            Vector3 previous = currentPos;
            for(int i = 0, length = mPath.corners.Length; i < length; i++)
            {
                Vector3 vector = mPath.corners[i];
                float distance = (vector - previous).magnitude;
                //skip points that would cause orientandpathto to fail
                if (length < MinTurningRad)
                    continue;

                //slightly shorten targets to make curving around corners feel more natural
                if (i != length-1)
                {
                    Vector3 toPrevious = previous - vector;
                    float radMult = -Mathf.Tan(
                        Mathf.Deg2Rad * Vector3.Angle(
                            toPrevious, (mPath.corners[i + 1] - vector)
                            )
                        );
                    
                }

                previous = vector;
                vector.y = catChestHeight;
                points.Add(vector);
            }
            destination.y = catChestHeight;
            PathToPoints(points, currentPos, currentHipPos, currentForward);
        }
        else
        {
            Debug.LogError("no path");
        }
    }
}
