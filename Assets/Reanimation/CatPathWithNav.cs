using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatPathWithNav : CatPath
{
    public float GroundHeight { get; set; }
    float catChestHeight;

    public CatPathWithNav(float catChestHeight, float[] delays, Transform[] transforms) : base(delays,transforms,0)
    {
        //default values for pathfinding settings
        MinTurningRad = .1f;
        Speed = .1f;
        this.catChestHeight = catChestHeight;
    }

    public override void PathToPoint(Vector3 destination)
    {
        Vector3 pathStart = shoulderTransform.position;
        pathStart.y = GroundHeight;
        destination.y = GroundHeight;

        //get a series of points from unity's navmesh
        NavMeshPath mPath = new NavMeshPath();
        if(NavMesh.CalculatePath(pathStart, destination, NavMesh.AllAreas, mPath))
        {
            var points = new List<Vector3>();
            Vector3 previous = shoulderTransform.position;
            for(int i = 1, length = mPath.corners.Length; i < length; i++)
            {
                Vector3 vector = mPath.corners[i];
                Vector3 toPrevious = previous - vector;
                float distance = toPrevious.magnitude;
                if (distance < MinTurningRad) //skip points that would cause orientandpathto to fail
                    continue;

                //slightly shorten targets to make curving around corners feel more natural
                if (i != length-1)
                {
                    float radMult = -Mathf.Tan(
                        Mathf.Deg2Rad * Vector3.Angle(
                            toPrevious, (mPath.corners[i + 1] - vector)
                            )
                        );
                    radMult = Mathf.Min(1, radMult);
                    vector += toPrevious * radMult * MinTurningRad / distance;
                }

                previous = vector;
                vector.y = catChestHeight;
                points.Add(vector);
            }
            destination.y = catChestHeight;
            PathToPoints(points);
        }
        else
        {
            Debug.LogError("no path");
        }
    }
}
