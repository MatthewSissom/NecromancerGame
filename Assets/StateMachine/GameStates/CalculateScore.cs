using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateScore : State
{
    public override IEnumerator Routine()
    {
        Begin();

        ScoreManager.Instance.Add(new PartialScore("head", 50));
        ScoreManager.Instance.Add(new PartialScore("legs"));
        ScoreManager.Instance.Add(new PartialScore("hips"));
        ScoreManager.Instance.Add(new PartialScore("ribs", 50));
        ScoreManager.Instance.Add(new PartialScore("ribs", 50));
        ScoreManager.Instance.Add(new PartialScore("ribs", 50));
        ScoreManager.Instance.Add(new PartialScore("ribs", 50));

        End();
        yield break;
    }
}
