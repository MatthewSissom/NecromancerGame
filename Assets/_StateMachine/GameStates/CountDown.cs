using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountDown : State
{
    public static CountDown instance;
    public TextMeshProUGUI text;
    public Stopwatch stopWatch;
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

    private void Update()
    {
        //jimmie is impatient when debugging on desktop
        if(Input.GetKeyDown(KeyCode.Q))
        {
            stopWatch.SetHandPercentage(360);
        }
    }
    public override IEnumerator Routine()
    {
        Begin();
        stopWatch.SetHandPercentage(0); 
        text.gameObject.SetActive(true);
        lidClosed = false;
        text.text = "Next Event: " + eventName;
        stopWatch.On = true;
        while (stopWatch.Angle < 360)
        {
            yield return null;
        }
        stopWatch.Angle = 0;
        stopWatch.On = false;
        text.text = "Finished!";
        stopWatch.SetHandPercentage(0);
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
