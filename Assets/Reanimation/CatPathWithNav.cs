using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatPathWithNav : CatPath
{
    public float GroundHeight { get; set; }
    float catChestHeight;

    public CatPathWithNav(float catChestHeight, float[] delays, Transform[] transforms, int shoulderIndex) : base(delays,transforms,shoulderIndex)
    {
        //default values for pathfinding settings
        MinTurningRad = .1f;
        Speed = .1f;
        this.catChestHeight = catChestHeight;
    }

    public bool PathToPoint(Vector3 destination)
    {
        Vector3 pathStart = shoulderTransform.position;
        pathStart.y = destination.y = GroundHeight;

        //get a series of points from unity's navmesh
        NavMeshPath mPath = new NavMeshPath();
        if (!NavMesh.CalculatePath(pathStart, destination, NavMesh.AllAreas, mPath))
        {
            Debug.LogError("no path");
            return false;
        }

        var points = new List<Vector3>();
        Vector3 previous = shoulderTransform.position;
        for (int i = 1, length = mPath.corners.Length-1; i < length; i++)
        {
            Vector3 vector = mPath.corners[i];
            Vector3 toPrevious = previous - vector;
            float distance = toPrevious.magnitude;

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
            vector.y = catChestHeight;
            points.Add(vector);
        }

        Vector3 lastVector = mPath.corners[mPath.corners.Length];
        lastVector.y = catChestHeight;
        points.Add(lastVector);

        return PathToPoints(points);
    }
}
