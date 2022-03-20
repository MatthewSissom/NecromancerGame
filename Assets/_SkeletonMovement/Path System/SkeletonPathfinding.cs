using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SkeletonPathfinding
{
    PathTunables tunables;

    public SkeletonPathfinding(PathTunables tunables)
    {
        this.tunables = tunables;
    }

    public List<Vector3> GetPathPoints(Vector3 pathStart, Vector3 destination)
    {
        //get a series of points from unity's navmesh
        NavMeshPath mPath = new NavMeshPath();
        if (!NavMesh.CalculatePath(pathStart, destination, NavMesh.AllAreas, mPath))
        {
            // no path, return
            return null;
        }

#if UNITY_EDITOR
        DebugRendering.UpdatePath(DebugModes.DebugPathFlags.NavMeshPath, mPath.corners);
#endif

        var points = new List<Vector3>();
        Vector3 previous = pathStart;
        for (int i = 1, length = mPath.corners.Length - 1; i < length; i++)
        {
            Vector3 vector = mPath.corners[i];
            Vector3 toPrevious = previous - vector;
            float distance = toPrevious.magnitude;

            // if jumping skip other tweaks to points
            float jumpHeight = -toPrevious.y;
            if (Mathf.Abs(jumpHeight) > .05f)
            {
                previous = vector;
                points.Add(vector);
                continue;
            }


            //slightly shorten targets to make curving around corners feel more natural
            float radMult = -Mathf.Tan(
                Mathf.Deg2Rad * Vector3.Angle(
                    toPrevious, (mPath.corners[i + 1] - vector)
                    )
                );
            radMult = Mathf.Min(1, radMult);
            vector += toPrevious / distance * radMult * tunables.MinTurningRad;

            //skip points that would cause orientandpathto to fail
            if ((previous - vector).magnitude < tunables.MinTurningRad)
                continue;

            previous = vector;

            points.Add(vector);
        }

        Vector3 lastVector = mPath.corners[mPath.corners.Length - 1];
        points.Add(lastVector);

#if UNITY_EDITOR
        DebugRendering.UpdatePath(DebugModes.DebugPathFlags.ModifiedNavMeshPath, points);
#endif

        return points;
    }
}
