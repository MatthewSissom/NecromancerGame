using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TutorialHelper
{
    static int instructionIndex = 0;
    public static IEnumerator DelayedWaitUntil(System.Func<bool> predicate)
    {
        yield return new WaitUntil(predicate);
        yield return new WaitForSeconds(1);
    }

    public static IEnumerator WaitForEquality<T>(System.Func<T> getter, T finalState)
    {
        yield return new WaitUntil(() => getter().Equals(finalState));
    }

    public static IEnumerator RepeatCoroutineUntil(System.Func<bool, IEnumerator> routine, System.Func<bool> isFinished)
    {
        // firstTry is true
        yield return routine(true);
        while (!isFinished())
        {
            // firstTry is false
            yield return routine(false);
        }
    }

    public static void PrepareNextInstruction(bool previousSucceded = true)
    {
        InputManager.Instance.enabled = false;

        if(previousSucceded)
            MenuInstructions.Instance.ShowInstruction(instructionIndex++);
        else
            MenuInstructions.Instance.ShowTutorialFailureScreen();
    }

    public static void DisableStopwatch()
    {

    }
}
