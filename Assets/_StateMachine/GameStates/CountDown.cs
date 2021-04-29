using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountDown : State
{
    public static CountDown instance;
    public TextMeshProUGUI text;
    string eventName = ""; 
    float time = 0;

    int warningTime = 5;    // The number of seconds reamining that is shown when the tick tock sound intensifies


    protected override void Awake()
    {
        base.Awake();
        if (instance)
            Destroy(this);
        else
            instance = this;
    }

    public override IEnumerator Routine()
    {
        Begin();

        text.gameObject.SetActive(true);
        text.text = "Next Event: " + eventName;
        yield return new WaitForSeconds(2);

        for (int i = (int)System.Math.Ceiling(time - 2); i > 0; i--)
        {
            text.text = eventName + " for " + i.ToString() + "s";

            if (i <= 10)
                AudioManager.Instance.PlayTickTock(true);
            else
                AudioManager.Instance.PlayTickTock(false);

            yield return new WaitForSeconds(1);
        }
        text.text = "Finished!";
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
