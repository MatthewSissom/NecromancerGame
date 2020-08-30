using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateScore : State
{
    public override IEnumerator Routine()
    {
        Begin();

        ScoreManager.Instance.Add(new PartialScore("Has a head!", 50));
        ScoreManager.Instance.Add(new PartialScore("No legs!"));
        ScoreManager.Instance.Add(new PartialScore("No hips"));
        ScoreManager.Instance.Add(new PartialScore("Has ribs!", 50));

        End();
        yield break;
    }
}
