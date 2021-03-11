using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Cat movement is in charge of the lowest level goals like moving a specific limb to a specific location
//Recives instrucitons from cat behavior
public class CatMovement
{
    GameObject mCat;

    public delegate void CommandsExaustedDelegate(LimbMovementCommand currentlyExecuting);
    public event CommandsExaustedDelegate CommandsExaustedEvent;

    public struct LimbMovementCommand
    {
        public LimbEnd limb;
        public Vector3 destination;

        public LimbMovementCommand(LimbEnd limb, Vector3 destination)
        {
            this.limb = limb;
            this.destination = destination;
        }
    }
    Queue<LimbMovementCommand> commands;

    int stablizerCount = 0;
    int maxStablizerCount = 0;


    public CatMovement(List<LimbEnd> limbEnds, GameObject mCat)
    {
        commands = new Queue<LimbMovementCommand>();
        this.mCat = mCat;

        foreach (var limb in limbEnds)
        {
            limb.StepStartEvent += LimbStartedStep;
            limb.StepEndEvent += LimbEndedStep;
        }

        stablizerCount = limbEnds.Count;
        maxStablizerCount = limbEnds.Count;
    }

    public void AddCommand(LimbEnd limb, Vector3 destination)
    {
        commands.Enqueue(new LimbMovementCommand(limb,destination));
    }

    public void StepWithNextLimb()
    {
        if (commands.Count == 0)
            return;

        LimbMovementCommand command = commands.Dequeue();
        command.limb.StartStep(command.destination);
        if (commands.Count == 0)
            CommandsExaustedEvent?.Invoke(command);
    }

    void LimbStartedStep(LimbEnd limb)
    {
        stablizerCount -= 1;
    }

    void LimbEndedStep(LimbEnd calling, Vector3? collisionPoint)
    {
        if (collisionPoint != null)
        {
            stablizerCount += 1;
            if (stablizerCount == maxStablizerCount)
            {
                StepWithNextLimb();
            }

            calling.StartPush();
        }
    }
}
