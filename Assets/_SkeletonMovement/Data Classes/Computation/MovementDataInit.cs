using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementDataInit : IAssemblyStage
{
    public SkeletonLayoutData ComputedLayoutData { get; private set; }
    public SkeletonPathData ComputedPathData { get; private set; }

    private LimbTunables limbTunables;
    private bool initalized;

    public MovementDataInit SetTunables(LimbTunables limbTunables)
    {
        this.limbTunables = limbTunables;
        return this;
    }

    public IEnumerator Execute(GameObject skeleton, IAssemblyStage previous)
    {
        // Create computers

        int limbCnt = 0;

        SpinePointHeight hipHeight      = new SpinePointHeight();
        SpinePointHeight shoulderHeight = new SpinePointHeight();
        LimbLength[] limbLengths        = DefaultArr<LimbLength>(limbCnt);
        StrideLenght[] strideLenghts    = DefaultArr<StrideLenght>(limbCnt);
        LimbIdData[] limbIdData         = DefaultArr<LimbIdData>(limbCnt);

        // Set relationships between computers
        SetArrData(strideLenghts, shoulderHeight, (StrideLenght sl, SpinePointHeight sh) => sl.SpinePointHeight = sh); ;


        initalized = true;

        yield break;
    }

    public SkeletonLayoutData EditorInit(LimbData[] limbEnds , SkeletonTransforms transforms, SkeletonTransforms targets, SkeletonPathTunables tunables)
    {
        Transform[] orderedTransforms = new Transform[4];
        orderedTransforms[0] = targets.Head;
        orderedTransforms[1] = targets.Shoulder;
        orderedTransforms[2] = targets.Hip;
        orderedTransforms[3] = targets.Tail;

        float[] distances = new float[4];
        distances[0] = 0;
        distances[1] = (orderedTransforms[0].transform.position - orderedTransforms[1].transform.position).magnitude;
        distances[2] = (orderedTransforms[1].transform.position - orderedTransforms[2].transform.position).magnitude;
        distances[3] = (orderedTransforms[2].transform.position - orderedTransforms[3].transform.position).magnitude;
        float totalDistance = distances[1] + distances[2] + distances[3];

        Transform GetMatchingTransform(SkeletonTransforms findFrom, LimbIdentityData identityData)
        {
            if(identityData.IsFront)
            {
                if (identityData.IsRight)
                {
                    return findFrom.FrontRightLeg;
                }
                return findFrom.FrontLeftLeg;
            }
            else
            {

                if (identityData.IsRight)
                {
                    return findFrom.BackRightLeg;
                }
                return findFrom.BackLeftLeg;
            }
        }

        // Create data
        SpinePointData[] spinePoints = new SpinePointData[orderedTransforms.Length];
        float cumulativeDelay = 0;
        for (int i = 0; i < spinePoints.Length; i++)
        {
            // simple delay calculation
            cumulativeDelay += distances[i] / tunables.Speed;

            SpineIdentityData idData = new SpineIdentityData(i == 1, i == 2);

            spinePoints[i] = new SpinePointData(
                orderedTransforms[i],
                cumulativeDelay,
                //temp
                .7f * .2f,
                idData
            );
        }

        // set limb delays to match their corresponding spine point (shoulders for front legs, hips for back legs)
        foreach (LimbData limbEnd in limbEnds)
        {
            Transform start = GetMatchingTransform(transforms, limbEnd.IdentityData);
            Transform target = GetMatchingTransform(targets, limbEnd.IdentityData);
            LimbTransforms limbGo = new LimbTransforms(start, target);

            if (limbEnd.IdentityData.IsFront)
            {
                limbEnd.EditorInit(limbGo, limbTunables, new LimbTracingData(spinePoints[1].Delay));
            }
            else
            {
                limbEnd.EditorInit(limbGo, limbTunables, new LimbTracingData(spinePoints[2].Delay));
            }
        }


        initalized = true;
        return new SkeletonLayoutData(limbEnds, spinePoints, totalDistance);
    }

    public bool ExecutedSuccessfully()
    {
        return initalized;
    }

    private T[] DefaultArr<T>(int len) where T : new()
    {
        T[] vals = new T[len];
        for(int i = 0; i < len; i++)
        {
            vals[i] = new T();
        }
        return vals;
    }

    private void SetArrData<T,V> (T[] toSet, V[] values, System.Action<T,V> func)
    {
        for(int i = 0; i < toSet.Length; i++)
        {
            func(toSet[i], values[i]);
        }
    }

    private void SetArrData<T, V>(T[] toSet, V value, System.Action<T, V> func)
    {
        for (int i = 0; i < toSet.Length; i++)
        {
            func(toSet[i], value);
        }
    }
}

