using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountDown : State
{
    public static CountDown instance;
    public TextMeshProUGUI text;
    public Stopwatch stopwatch;
    public StopwatchLid lid;

    string eventName = ""; 
    float time = 0;

    int warningTime = 5;    // The number of seconds reamining that is shown when the tick tock sound intensifies
    bool lidClosed = false;

    protected override void Awake()
    {
        base.Awake();
        if (instance)
            Destroy(this);
        else
            instance = this;
    }

    private void Start()
    {
        lid.LidClosed += () => { lidClosed = true; };
    }

    public override IEnumerator Routine()
    {
        Begin();

        stopwatch.SetHandPercentage(0); 
        text.gameObject.SetActive(true);
        lidClosed = false;
        text.text = "Next Event: " + eventName;
        yield return new WaitForSeconds(1);
        stopwatch.SetHandPercentage(1 / time); 
        yield return new WaitForSeconds(1);
        stopwatch.SetHandPercentage(2 / time);

        for (int i = (int)System.Math.Ceiling(time - 2); i > 0; i--)
        {
            if (lidClosed)
                break;

            text.text = eventName + "!";

            if (i <= warningTime)
                AudioManager.Instance.PlayTickTock(true);
            else
                AudioManager.Instance.PlayTickTock(false);

            yield return new WaitForSeconds(1);
            stopwatch.SetHandPercentage((time-i+1) / time);
        }
        text.text = "Finished!";
        stopwatch.SetHandPercentage(0); 
        yield return new WaitForSeconds(1);
        text.gameObject.SetActive(false);

        End();
        yield return null;
    }

    public static void SetParams(string eventName, float time)
    {
        instance.eventName = eventName;
        instance.time = time;
    }
}
