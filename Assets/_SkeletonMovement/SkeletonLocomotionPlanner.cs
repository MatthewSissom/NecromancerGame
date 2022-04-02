using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonLocomotionPlanner
{
    private SkeletonBasePathBuilder pathBuilder;
    private SkeletonLayoutData layoutData;

    public SkeletonLocomotionPlanner(SkeletonBasePathBuilder pathBuilder, SkeletonLayoutData layoutData)
    {
        this.pathBuilder = pathBuilder;
        this.layoutData = layoutData;
    }

    public Queue<Action> PlanMovementFromDestinations(List<Vector3> destinations)
    {
        if (destinations.Count < 2)
            return null;


        Queue<Action> actions = new Queue<Action>();
        int walkStartIndex = -1;
        for(int i = 1; i < destinations.Count; i++)
        {
            Vector3 previousDest = destinations[i - 1];
            Vector3 currentDest = destinations[i];

            // endIndex is EXCLUSIVE
            void FinishWalkIfNeeded(int endIndex)
            {
                if (walkStartIndex == -1 || endIndex == walkStartIndex)
                    return;

                Vector3[] walkDestinations = new Vector3[endIndex - walkStartIndex];
                for(int j = walkStartIndex; j < endIndex; j++)
                {
                    walkDestinations[j - walkStartIndex] = destinations[j];
                }

                actions.Enqueue(new WalkAction(walkDestinations, pathBuilder, layoutData));
            }

            // jump needed
            if(Mathf.Abs(previousDest.y - currentDest.y) > .07f)
            {
                // walk to jump point if needed
                if (walkStartIndex == -1 && i != 1)
                {
                    walkStartIndex = 0;
                }
                FinishWalkIfNeeded(i);
                walkStartIndex = i;

                //TODO add jumps
            }
            else if (i == destinations.Count -1)
            {
                // no other paths were added, ok to walk between all destinations
                if (walkStartIndex == -1)
                    walkStartIndex = 0;
                FinishWalkIfNeeded(destinations.Count);
            }
        }

        return actions;
    }
}
